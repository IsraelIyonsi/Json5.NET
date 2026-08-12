using Json5;

namespace Json5.Tests.Errors;

/// <summary>
/// Verifies exact one-based line/column positions for a curated set of malformed inputs,
/// computed by hand against this library's own line/column contract. These are independent
/// of the json5-tests corpus's own <c>.errorSpec</c> files, whose "at"/"lineNumber" fields
/// encode the reference JavaScript implementation's internal, non-standardized diagnostics
/// and are not a contract this library commits to matching byte-for-byte.
/// </summary>
public sealed class Json5ErrorPositionTests
{
    [Fact]
    public void MissingValueAfterColon_PointsAtTheOffendingToken()
    {
        var exception = Assert.Throws<Json5Exception>(() => Json5.Parse("{\n  \"a\": }"));

        Assert.Equal(2, exception.Line);
        Assert.Equal(8, exception.Column);
    }

    [Fact]
    public void UnterminatedString_PointsAtTheOpeningQuote()
    {
        var exception = Assert.Throws<Json5Exception>(() => Json5.Parse("\"abc"));

        Assert.Equal(1, exception.Line);
        Assert.Equal(1, exception.Column);
    }

    [Fact]
    public void MissingCommaBetweenArrayElements_PointsAtTheSecondElement()
    {
        var exception = Assert.Throws<Json5Exception>(() => Json5.Parse("[\n  1\n  2\n]"));

        Assert.Equal(3, exception.Line);
        Assert.Equal(3, exception.Column);
    }

    [Fact]
    public void LeadingZeroFollowedByDigit_PointsAtTheExtraDigit()
    {
        var exception = Assert.Throws<Json5Exception>(() => Json5.Parse("010"));

        Assert.Equal(1, exception.Line);
        Assert.Equal(2, exception.Column);
    }

    [Fact]
    public void UnterminatedBlockComment_PointsAtCommentStart()
    {
        var exception = Assert.Throws<Json5Exception>(() => Json5.Parse("true\n/* never closed"));

        Assert.Equal(2, exception.Line);
        Assert.Equal(1, exception.Column);
    }

    [Fact]
    public void UnexpectedCharacter_PointsAtThatCharacter()
    {
        var exception = Assert.Throws<Json5Exception>(() => Json5.Parse("{\"a\": #}"));

        Assert.Equal(1, exception.Line);
        Assert.Equal(7, exception.Column);
    }

    [Fact]
    public void CrlfLineEndings_AdvanceLineNumberOnceNotTwice()
    {
        var exception = Assert.Throws<Json5Exception>(() => Json5.Parse("[\r\n  1\r\n  2\r\n]"));

        Assert.Equal(3, exception.Line);
    }

    [Fact]
    public void MissingDigitAfterDecimalPoint_PointsAfterTheDot_NotAtTheNumbersStart()
    {
        var exception = Assert.Throws<Json5Exception>(() => Json5.Parse("-."));

        Assert.Equal(1, exception.Line);
        Assert.Equal(3, exception.Column);
    }

    [Fact]
    public void MissingDigitInExponent_PointsAfterTheExponentMarker_NotAtTheNumbersStart()
    {
        var exception = Assert.Throws<Json5Exception>(() => Json5.Parse("1e"));

        Assert.Equal(1, exception.Line);
        Assert.Equal(3, exception.Column);
    }

    [Fact]
    public void MissingHexDigitAfterZeroX_PointsAfterTheXPrefix_NotAtTheNumbersStart()
    {
        var exception = Assert.Throws<Json5Exception>(() => Json5.Parse("0x"));

        Assert.Equal(1, exception.Line);
        Assert.Equal(3, exception.Column);
    }
}
