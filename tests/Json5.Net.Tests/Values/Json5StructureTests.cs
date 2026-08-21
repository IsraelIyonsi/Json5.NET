using System.Text;
using System.Text.Json.Nodes;
using Json5;

namespace Json5.Tests.Values;

/// <summary>
/// Table-driven coverage of JSON5 document structure: comments, trailing commas, unquoted and
/// reserved-word object keys, duplicate keys, top-level scalars, and the nesting depth guard.
/// </summary>
public sealed class Json5StructureTests
{
    [Theory]
    [InlineData("// leading line comment\n{\"a\":1}")]
    [InlineData("{\"a\":1}\n// trailing line comment")]
    [InlineData("/* leading block comment */{\"a\":1}")]
    [InlineData("{\"a\":1}/* trailing block comment */")]
    [InlineData("{\n  // comment before member\n  \"a\": 1\n}")]
    [InlineData("{\n  \"a\": 1 // comment after member\n}")]
    [InlineData("{/* a */\"a\"/* b */:/* c */1/* d */}")]
    public void CommentsAreInsignificantWhitespace(string json5)
    {
        var node = Json5Convert.Parse(json5)!.AsObject();

        Assert.Equal(1, node["a"]!.GetValue<int>());
    }

    [Theory]
    [InlineData("[1,2,3,]", 3)]
    [InlineData("[1,]", 1)]
    [InlineData("[]", 0)]
    public void ArraysAllowTrailingComma(string json5, int expectedCount)
    {
        var node = Json5Convert.Parse(json5)!.AsArray();

        Assert.Equal(expectedCount, node.Count);
    }

    [Fact]
    public void ObjectsAllowTrailingComma()
    {
        var node = Json5Convert.Parse("{\"a\":1,\"b\":2,}")!.AsObject();

        Assert.Equal(2, node.Count);
    }

    [Theory]
    [InlineData("{hello:\"world\"}", "hello")]
    [InlineData("{_:\"underscore\"}", "_")]
    [InlineData("{$:\"dollar\"}", "$")]
    [InlineData("{_$_:\"combo\"}", "_$_")]
    [InlineData("{while:true}", "while")]
    [InlineData("{'single':1}", "single")]
    [InlineData("{Ⅵ:6}", "Ⅵ")]
    [InlineData("{a१:1}", "a१")]
    public void UnquotedAndSingleQuotedKeysAreAccepted(string json5, string expectedKey)
    {
        var node = Json5Convert.Parse(json5)!.AsObject();

        Assert.True(node.ContainsKey(expectedKey));
    }

    [Fact]
    public void DuplicateKeys_LastValueWins()
    {
        var node = Json5Convert.Parse("{\"a\":true,\"a\":false}")!.AsObject();

        Assert.False(node["a"]!.GetValue<bool>());
    }

    [Theory]
    [InlineData("null", null)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void TopLevelScalarsAreValid(string json5, bool? expected)
    {
        var node = Json5Convert.Parse(json5);

        if (expected is null)
        {
            Assert.Null(node);
        }
        else
        {
            Assert.Equal(expected, node!.GetValue<bool>());
        }
    }

    [Fact]
    public void TopLevelString_IsValid()
    {
        var node = Json5Convert.Parse("'top level string'");

        Assert.Equal("top level string", node!.GetValue<string>());
    }

    [Fact]
    public void NestedObjectsAndArrays_ParseCorrectly()
    {
        const string json5 = """
            {
              users: [
                { name: 'Ada', roles: ['admin', 'auditor'] },
                { name: 'Grace', roles: [] },
              ],
              count: 2,
            }
            """;

        var root = Json5Convert.Parse(json5)!.AsObject();
        var users = root["users"]!.AsArray();

        Assert.Equal(2, root["count"]!.GetValue<int>());
        Assert.Equal("Ada", users[0]!["name"]!.GetValue<string>());
        Assert.Equal(["admin", "auditor"], users[0]!["roles"]!.AsArray().Select(n => n!.GetValue<string>()));
        Assert.Empty(users[1]!["roles"]!.AsArray());
    }

    [Fact]
    public void NestingWithinMaxDepth_Parses()
    {
        string json5 = BuildNestedArray(60);

        var exception = Record.Exception(() => Json5Convert.Parse(json5));

        Assert.Null(exception);
    }

    [Fact]
    public void NestingBeyondMaxDepth_Throws()
    {
        string json5 = BuildNestedArray(1000);

        Assert.Throws<Json5Exception>(() => Json5Convert.Parse(json5));
    }

    [Theory]
    [InlineData("{,}")]
    [InlineData("{,\"foo\":\"bar\"}")]
    [InlineData("{\"foo\":\"bar\" \"hello\":\"world\"}")]
    [InlineData("[,]")]
    [InlineData("[,null]")]
    [InlineData("[true false]")]
    [InlineData("{10twenty:\"ten twenty\"}")]
    [InlineData("{multi-word:\"multi-word\"}")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("// only a comment")]
    [InlineData("/* only a comment */")]
    public void MalformedStructure_Throws(string json5)
    {
        Assert.Throws<Json5Exception>(() => Json5Convert.Parse(json5));
    }

    [Fact]
    public void NelIsNotTreatedAsInsignificantWhitespace()
    {
        // U+0085 (NEL) is not in the JSON5 JSON5Whitespace/JSON5LineTerminator productions,
        // unlike System.Char.IsWhiteSpace, which over-accepts it.
        var exception = Assert.Throws<Json5Exception>(() => Json5Convert.Parse("\u0085true"));

        Assert.Equal(1, exception.Line);
        Assert.Equal(1, exception.Column);
    }

    private static string BuildNestedArray(int depth)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < depth; i++)
        {
            sb.Append('[');
        }

        sb.Append('1');
        for (int i = 0; i < depth; i++)
        {
            sb.Append(']');
        }

        return sb.ToString();
    }
}
