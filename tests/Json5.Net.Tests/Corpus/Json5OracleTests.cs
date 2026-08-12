using System.Text.Json.Nodes;
using Json5;

namespace Json5.Tests.Corpus;

/// <summary>
/// Cross-checks parsing against reference material shipped inside the official json5-tests
/// corpus (https://github.com/json5/json5-tests, MIT licensed): a JSON5 document paired with
/// its strict-JSON equivalent, and the canonical example from the JSON5 specification site.
/// </summary>
public sealed class Json5OracleTests
{
    private static readonly string FixturesRoot =
        Path.Combine(AppContext.BaseDirectory, "fixtures", "json5-tests");

    [Fact]
    public void NpmPackageJson5_MatchesStrictJsonEquivalent()
    {
        string json5Text = File.ReadAllText(Path.Combine(FixturesRoot, "misc", "npm-package.json5"));
        string strictJsonText = File.ReadAllText(Path.Combine(FixturesRoot, "misc", "npm-package.json"));

        JsonNode? fromJson5 = Json5.Parse(json5Text);
        JsonNode? fromStrictJson = JsonNode.Parse(strictJsonText);

        Assert.True(
            JsonNode.DeepEquals(fromJson5, fromStrictJson),
            "Parsing the JSON5 form should produce a tree identical to parsing its hand-written strict-JSON equivalent.");
    }

    [Fact]
    public void ReadmeExample_MatchesEveryDocumentedValue()
    {
        string text = File.ReadAllText(Path.Combine(FixturesRoot, "misc", "readme-example.json5"));

        var root = Json5.Parse(text)!.AsObject();

        Assert.Equal("bar", root["foo"]!.GetValue<string>());
        Assert.True(root["while"]!.GetValue<bool>());
        Assert.Equal("is a multi-line string", root["this"]!.GetValue<string>());
        Assert.Equal("is another", root["here"]!.GetValue<string>());
        Assert.Equal(0xDEADBEEFU, root["hex"]!.GetValue<uint>());
        Assert.Equal(0.5, root["half"]!.GetValue<double>());
        Assert.Equal(10, root["delta"]!.GetValue<int>());
        Assert.Equal(double.PositiveInfinity, root["to"]!.GetValue<double>());
        Assert.Equal("a trailing comma", root["finally"]!.GetValue<string>());

        var array = root["oh"]!.AsArray();
        Assert.Equal(
            ["we shouldn't forget", "arrays can have", "trailing commas too"],
            array.Select(n => n!.GetValue<string>()));
    }
}
