using System.Text.Json;
using Json5;

namespace Json5.Tests.Api;

/// <summary>
/// Coverage of <see cref="Json5.Deserialize{T}(string, System.Text.Json.JsonSerializerOptions?)"/>:
/// deserializing JSON5 config text directly into plain .NET types via System.Text.Json.
/// </summary>
public sealed class Json5DeserializeTests
{
    private static readonly JsonSerializerOptions CaseInsensitive =
        new() { PropertyNameCaseInsensitive = true };

    private sealed record ServerConfig(string Host, int Port, bool UseTls, string[] AllowedOrigins);

    private sealed class Threshold
    {
        public double Warning { get; set; }

        public double Critical { get; set; }
    }

    [Fact]
    public void DeserializesIntoRecordFromRelaxedJson5()
    {
        const string json5 = """
            {
              // Server binding
              host: 'localhost',
              port: 8080,
              useTls: false,
              allowedOrigins: [
                'https://example.com',
                'https://app.example.com',
              ],
            }
            """;

        var config = Json5.Deserialize<ServerConfig>(json5, CaseInsensitive);

        Assert.Equal("localhost", config!.Host);
        Assert.Equal(8080, config.Port);
        Assert.False(config.UseTls);
        Assert.Equal(["https://example.com", "https://app.example.com"], config.AllowedOrigins);
    }

    [Fact]
    public void DeserializesInfinityIntoDoubleProperty()
    {
        const string json5 = "{ warning: 0.8, critical: Infinity }";

        var threshold = Json5.Deserialize<Threshold>(json5, CaseInsensitive);

        Assert.Equal(0.8, threshold!.Warning);
        Assert.Equal(double.PositiveInfinity, threshold.Critical);
    }

    [Fact]
    public void DeserializesIntoDictionary()
    {
        const string json5 = "{a: 1, b: 2, c: 3}";

        var map = Json5.Deserialize<Dictionary<string, int>>(json5);

        Assert.Equal(new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 }, map);
    }

    [Fact]
    public void DeserializesIntoListOfInt()
    {
        const string json5 = "[1, 2, 3,]";

        var list = Json5.Deserialize<List<int>>(json5);

        Assert.Equal([1, 2, 3], list);
    }

    [Fact]
    public void DeserializesTopLevelNull_ReturnsDefaultForReferenceType()
    {
        var result = Json5.Deserialize<ServerConfig>("null");

        Assert.Null(result);
    }

    [Fact]
    public void DeserializesPrimitiveInt()
    {
        var result = Json5.Deserialize<int>("42");

        Assert.Equal(42, result);
    }

    [Fact]
    public void MalformedInput_ThrowsJson5ExceptionNotSilently()
    {
        Assert.Throws<Json5Exception>(() => Json5.Deserialize<ServerConfig>("{host:}"));
    }

    [Fact]
    public void RepeatedDeserializeCalls_ReuseTheSameAugmentedOptionsInstance()
    {
        // Deserialize<T> augments caller-supplied options with AllowNamedFloatingPointLiterals
        // when it's missing. That augmented clone is cached per caller-options instance so
        // repeated calls reuse System.Text.Json's JsonTypeInfo cache instead of discarding it
        // on every call.
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var method = typeof(Json5).GetMethod(
            "WithNamedFloatingPointLiterals",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;

        var first = method.Invoke(null, [options]);
        var second = method.Invoke(null, [options]);

        Assert.NotSame(options, first);
        Assert.Same(first, second);
    }
}
