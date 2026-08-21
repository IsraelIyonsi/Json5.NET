// Regression: the facade type must be reachable via 'using Json5;' + simple name (namespace==class collision).
using Json5;

namespace ConsumerScenario;

/// <summary>
/// Reproduces exactly how an external consumer references the facade: a namespace that is not
/// nested under <c>Json5</c>, a top-level <c>using Json5;</c>, and calls to the type by its
/// simple name. If the facade ever collides with the namespace again, this file fails to compile.
/// </summary>
public sealed class ConsumerScenarioTests
{
    [Fact]
    public void Parse_BySimpleName_ReturnsParsedValue()
    {
        var node = Json5Convert.Parse("{a:1}");

        Assert.Equal(1, node!["a"]!.GetValue<int>());
    }

    [Fact]
    public void TryParse_BySimpleName_ReturnsTrueWithParsedValue()
    {
        bool succeeded = Json5Convert.TryParse("{a:1}", out var result);

        Assert.True(succeeded);
        Assert.Equal(1, result!["a"]!.GetValue<int>());
    }
}
