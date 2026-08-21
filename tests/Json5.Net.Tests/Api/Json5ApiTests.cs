using Json5;

namespace Json5.Tests.Api;

/// <summary>
/// Coverage of the <see cref="Json5Convert"/> static API surface itself: null handling, the
/// <c>TryParse</c> non-throwing variant, and its documented ambiguity with JSON5 <c>null</c>.
/// </summary>
public sealed class Json5ApiTests
{
    [Fact]
    public void Parse_NullText_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Json5Convert.Parse(null!));
    }

    [Fact]
    public void TryParse_NullText_ReturnsFalse()
    {
        bool succeeded = Json5Convert.TryParse(null, out var result);

        Assert.False(succeeded);
        Assert.Null(result);
    }

    [Fact]
    public void TryParse_ValidText_ReturnsTrueWithParsedValue()
    {
        bool succeeded = Json5Convert.TryParse("{a:1}", out var result);

        Assert.True(succeeded);
        Assert.Equal(1, result!["a"]!.GetValue<int>());
    }

    [Fact]
    public void TryParse_InvalidText_ReturnsFalseWithNullResult()
    {
        bool succeeded = Json5Convert.TryParse("{a:}", out var result);

        Assert.False(succeeded);
        Assert.Null(result);
    }

    [Fact]
    public void TryParse_TopLevelNullLiteral_ReturnsTrueWithNullResult()
    {
        bool succeeded = Json5Convert.TryParse("null", out var result);

        Assert.True(succeeded);
        Assert.Null(result);
    }

    [Fact]
    public void Parse_TopLevelNullLiteral_ReturnsNull()
    {
        var result = Json5Convert.Parse("null");

        Assert.Null(result);
    }

    [Fact]
    public void Parse_ThrownExceptionIsJsonException()
    {
        var exception = Assert.Throws<Json5Exception>(() => Json5Convert.Parse("{"));

        Assert.IsAssignableFrom<System.Text.Json.JsonException>(exception);
    }
}
