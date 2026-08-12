using System.Globalization;

namespace Json5;

/// <summary>
/// ECMAScript character-class predicates used by the tokenizer. Internal: none of this is public API.
/// </summary>
internal static class Json5CharacterClassification
{
    public static bool IsDecimalDigit(char? c) => c is >= '0' and <= '9';

    public static bool IsHexDigit(char? c) =>
        c is (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F');

    public static int HexDigitValue(char c) => c switch
    {
        >= '0' and <= '9' => c - '0',
        >= 'a' and <= 'f' => c - 'a' + 10,
        >= 'A' and <= 'F' => c - 'A' + 10,
        _ => throw new ArgumentOutOfRangeException(nameof(c), c, "Not a hexadecimal digit."),
    };

    public static bool IsLineTerminator(char c) =>
        c is '\n' or '\r' or Json5Constants.LineSeparator or Json5Constants.ParagraphSeparator;

    /// <summary>
    /// Matches exactly the JSON5 <c>JSON5InputElement</c> trivia set: <c>TAB</c>, <c>LF</c>,
    /// <c>VT</c>, <c>FF</c>, <c>CR</c>, <c>SP</c>, <c>NBSP</c>, <c>BOM</c>, Unicode category
    /// <c>Zs</c>, <c>LS</c> and <c>PS</c>. Deliberately narrower than <see cref="char.IsWhiteSpace(char)"/>,
    /// which also accepts U+0085 (NEL), a character the JSON5 grammar does not treat as whitespace.
    /// </summary>
    public static bool IsWhitespace(char c) => c switch
    {
        '\t' or '\n' or '\v' or '\f' or '\r' or ' ' => true,
        Json5Constants.ByteOrderMark or Json5Constants.LineSeparator or Json5Constants.ParagraphSeparator => true,
        _ => CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator,
    };

    /// <summary>
    /// Matches the ECMAScript <c>IdentifierStart</c> production: Unicode categories
    /// Lu/Ll/Lt/Lm/Lo (<see cref="char.IsLetter(char)"/>) plus Nl (letter numbers, e.g. Ⅵ), or
    /// <c>$</c>/<c>_</c>.
    /// </summary>
    public static bool IsIdentifierStart(char c) =>
        char.IsLetter(c) || c is '$' or '_' || CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.LetterNumber;

    /// <summary>
    /// Matches the ECMAScript <c>IdentifierPart</c> production: everything <see cref="IsIdentifierStart"/>
    /// accepts, plus combining marks (Mn/Mc), decimal digits of any script (Nd, not just ASCII
    /// 0-9), connector punctuation (Pc), and the zero-width non-joiner/joiner.
    /// </summary>
    public static bool IsIdentifierPart(char c)
    {
        if (IsIdentifierStart(c))
        {
            return true;
        }

        if (c is Json5Constants.ZeroWidthNonJoiner or Json5Constants.ZeroWidthJoiner)
        {
            return true;
        }

        var category = CharUnicodeInfo.GetUnicodeCategory(c);
        return category is UnicodeCategory.NonSpacingMark
            or UnicodeCategory.SpacingCombiningMark
            or UnicodeCategory.ConnectorPunctuation
            or UnicodeCategory.DecimalDigitNumber;
    }
}
