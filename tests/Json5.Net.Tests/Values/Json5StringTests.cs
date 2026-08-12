using Json5;

namespace Json5.Tests.Values;

/// <summary>
/// Table-driven coverage of JSON5 string literals: single and double quotes, the full escape
/// sequence set, line continuations, and the rule that a raw line terminator ends the string
/// literal grammar with an error.
/// </summary>
public sealed class Json5StringTests
{
    [Theory]
    [InlineData("\"hello world\"", "hello world")]
    [InlineData("'hello world'", "hello world")]
    [InlineData("'I can\\'t wait'", "I can't wait")]
    [InlineData("\"quote: \\\"\"", "quote: \"")]
    [InlineData(@"'tab:\t'", "tab:\t")]
    [InlineData(@"'newline:\n'", "newline:\n")]
    [InlineData(@"'return:\r'", "return:\r")]
    [InlineData(@"'backspace:\b'", "backspace:\b")]
    [InlineData(@"'formfeed:\f'", "formfeed:\f")]
    [InlineData(@"'vtab:\v'", "vtab:\v")]
    [InlineData(@"'nul:\0end'", "nul:\0end")]
    [InlineData(@"'hex:\x41'", "hex:A")]
    [InlineData(@"'unicode:\u0041'", "unicode:A")]
    [InlineData(@"'nonescape:\q'", "nonescape:q")]
    [InlineData("'line\\\ncontinuation'", "linecontinuation")]
    [InlineData("'line\\\r\ncontinuation'", "linecontinuation")]
    [InlineData("'line\\\rcontinuation'", "linecontinuation")]
    public void ParsesToExpectedString(string json5, string expected)
    {
        var node = Json5.Parse(json5);

        Assert.Equal(expected, node!.GetValue<string>());
    }

    [Fact]
    public void EscapedSingleQuotedStringFixture_MatchesSpec()
    {
        var node = Json5.Parse("'I can\\'t wait'");

        Assert.Equal("I can't wait", node!.GetValue<string>());
    }

    [Fact]
    public void UnicodeEscape_SupportsSurrogatePairs()
    {
        var node = Json5.Parse("'\\uD83D\\uDE00'");

        Assert.Equal("\uD83D\uDE00", node!.GetValue<string>());
    }

    [Theory]
    [InlineData("\"unterminated")]
    [InlineData("'unterminated")]
    [InlineData("\"raw\nlinebreak\"")]
    [InlineData("\"bad hex \\x4\"")]
    [InlineData("\"bad unicode \\u004\"")]
    [InlineData("\"octal \\01\"")]
    [InlineData("'\\1'")]
    [InlineData("'\\9'")]
    [InlineData("\"\\5\"")]
    [InlineData("'digit \\3 mid-string'")]
    public void InvalidStringLiteral_Throws(string json5)
    {
        Assert.Throws<Json5Exception>(() => Json5.Parse(json5));
    }
}
