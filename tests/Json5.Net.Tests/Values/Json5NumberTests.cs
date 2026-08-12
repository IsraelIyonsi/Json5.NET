using Json5;

namespace Json5.Tests.Values;

/// <summary>
/// Table-driven coverage of the JSON5 number grammar: hexadecimal, leading/trailing decimal
/// points, explicit signs, exponents, signed Infinity/NaN, and the leading-zero restrictions
/// JSON5 inherits from ECMAScript.
/// </summary>
public sealed class Json5NumberTests
{
    [Theory]
    [InlineData("0", 0.0)]
    [InlineData("15", 15.0)]
    [InlineData("+15", 15.0)]
    [InlineData("-15", -15.0)]
    [InlineData("1.2", 1.2)]
    [InlineData("-1.2", -1.2)]
    [InlineData(".5", 0.5)]
    [InlineData("+.5", 0.5)]
    [InlineData("-.5", -0.5)]
    [InlineData("5.", 5.0)]
    [InlineData("+5.", 5.0)]
    [InlineData("-5.", -5.0)]
    [InlineData("0.5", 0.5)]
    [InlineData(".0", 0.0)]
    [InlineData("0.", 0.0)]
    [InlineData("2e23", 2e23)]
    [InlineData("2e-23", 2e-23)]
    [InlineData("1e+2", 1e2)]
    [InlineData("5e0", 5.0)]
    [InlineData("5e-0", 5.0)]
    [InlineData("5e+0", 5.0)]
    [InlineData("5.e4", 50000.0)]
    [InlineData("0e23", 0.0)]
    [InlineData("0xC8", 200.0)]
    [InlineData("0Xc8", 200.0)]
    [InlineData("0xc8e4", 51428.0)]
    [InlineData("-0xC8", -200.0)]
    [InlineData("+0xC8", 200.0)]
    [InlineData("0x0", 0.0)]
    public void ParsesToExpectedDouble(string json5, double expected)
    {
        var node = Json5.Parse(json5);

        Assert.Equal(expected, node!.GetValue<double>());
    }

    [Theory]
    [InlineData("Infinity", double.PositiveInfinity)]
    [InlineData("+Infinity", double.PositiveInfinity)]
    [InlineData("-Infinity", double.NegativeInfinity)]
    [InlineData("NaN", double.NaN)]
    [InlineData("+NaN", double.NaN)]
    [InlineData("-NaN", double.NaN)]
    public void ParsesSignedInfinityAndNaN(string json5, double expected)
    {
        var node = Json5.Parse(json5);

        Assert.Equal(expected, node!.GetValue<double>());
    }

    [Fact]
    public void NegativeZeroFloat_PreservesSign()
    {
        var node = Json5.Parse("-0.0");

        Assert.Equal(0.0, node!.GetValue<double>());
        Assert.True(double.IsNegative(node.GetValue<double>()));
    }

    [Fact]
    public void NegativeZeroHex_PreservesSign()
    {
        var node = Json5.Parse("-0x0");

        Assert.Equal(0.0, node!.GetValue<double>());
        Assert.True(double.IsNegative(node.GetValue<double>()));
    }

    [Fact]
    public void LargeIntegerLiteral_RoundTripsExactlyAsLong()
    {
        var node = Json5.Parse("9007199254740993");

        Assert.Equal(9007199254740993L, node!.GetValue<long>());
    }

    [Theory]
    [InlineData("010")]
    [InlineData("00")]
    [InlineData("+00")]
    [InlineData("-00")]
    [InlineData("-0123")]
    [InlineData("+0123")]
    [InlineData("080")]
    [InlineData("0780")]
    [InlineData("+098")]
    [InlineData("-098")]
    [InlineData(".")]
    [InlineData("0x")]
    [InlineData("+foo")]
    [InlineData("1e2.3")]
    [InlineData("1e")]
    [InlineData("1e+")]
    public void InvalidNumberLiteral_Throws(string json5)
    {
        Assert.Throws<Json5Exception>(() => Json5.Parse(json5));
    }
}
