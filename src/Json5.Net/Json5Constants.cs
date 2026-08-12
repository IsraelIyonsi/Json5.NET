namespace Json5;

/// <summary>
/// Named constants shared by the tokenizer and parser. Internal: none of this is public API.
/// </summary>
internal static class Json5Constants
{
    /// <summary>Maximum object/array nesting depth, matching the System.Text.Json JsonDocument default.</summary>
    public const int MaxNestingDepth = 64;

    /// <summary>Number of hex digits in a backslash-x escape in a string literal.</summary>
    public const int HexEscapeDigitCount = 2;

    /// <summary>Number of hex digits in a backslash-u escape in a string literal or identifier.</summary>
    public const int UnicodeEscapeDigitCount = 4;

    /// <summary>Base used when accumulating hexadecimal digit values.</summary>
    public const int HexRadix = 16;

    /// <summary>Unicode code point of the byte order mark / zero width no-break space, whitespace per the ECMAScript grammar.</summary>
    public const int ByteOrderMarkCodePoint = 0xFEFF;

    /// <summary>Unicode code point of the line separator, a JSON5 line terminator.</summary>
    public const int LineSeparatorCodePoint = 0x2028;

    /// <summary>Unicode code point of the paragraph separator, a JSON5 line terminator.</summary>
    public const int ParagraphSeparatorCodePoint = 0x2029;

    /// <summary>Unicode code point of the zero width non-joiner, a valid ECMAScript identifier part character.</summary>
    public const int ZeroWidthNonJoinerCodePoint = 0x200C;

    /// <summary>Unicode code point of the zero width joiner, a valid ECMAScript identifier part character.</summary>
    public const int ZeroWidthJoinerCodePoint = 0x200D;

    /// <summary>The byte order mark / zero width no-break space, whitespace per the ECMAScript grammar.</summary>
    public const char ByteOrderMark = (char)ByteOrderMarkCodePoint;

    /// <summary>The Unicode line separator, a JSON5 line terminator.</summary>
    public const char LineSeparator = (char)LineSeparatorCodePoint;

    /// <summary>The Unicode paragraph separator, a JSON5 line terminator.</summary>
    public const char ParagraphSeparator = (char)ParagraphSeparatorCodePoint;

    /// <summary>Zero width non-joiner, a valid ECMAScript identifier part character.</summary>
    public const char ZeroWidthNonJoiner = (char)ZeroWidthNonJoinerCodePoint;

    /// <summary>Zero width joiner, a valid ECMAScript identifier part character.</summary>
    public const char ZeroWidthJoiner = (char)ZeroWidthJoinerCodePoint;

    /// <summary>The JSON5 literal spelling of the boolean true value.</summary>
    public const string TrueKeyword = "true";

    /// <summary>The JSON5 literal spelling of the boolean false value.</summary>
    public const string FalseKeyword = "false";

    /// <summary>The JSON5 literal spelling of the null value.</summary>
    public const string NullKeyword = "null";

    /// <summary>The JSON5 literal spelling of positive/negative infinity.</summary>
    public const string InfinityKeyword = "Infinity";

    /// <summary>The JSON5 literal spelling of not-a-number.</summary>
    public const string NanKeyword = "NaN";
}
