namespace Json5;

/// <summary>
/// The lexical categories produced by <see cref="Json5Lexer"/>. Internal: none of this is public API.
/// </summary>
internal enum Json5TokenKind
{
    EndOfInput,
    LeftBrace,
    RightBrace,
    LeftBracket,
    RightBracket,
    Colon,
    Comma,
    StringLiteral,
    NumberLiteral,
    Identifier,
}

/// <summary>
/// The shape a <see cref="Json5TokenKind.NumberLiteral"/> token takes. Internal: none of this is public API.
/// </summary>
internal enum Json5NumberForm
{
    Decimal,
    PositiveInfinity,
    NegativeInfinity,
    NaN,
}

/// <summary>
/// A single lexical token together with its one-based source position. Internal: none of this is public API.
/// </summary>
internal sealed class Json5Token
{
    private Json5Token(Json5TokenKind kind, string text, Json5NumberForm numberForm, int line, int column)
    {
        Kind = kind;
        Text = text;
        NumberForm = numberForm;
        Line = line;
        Column = column;
    }

    public Json5TokenKind Kind { get; }

    public string Text { get; }

    public Json5NumberForm NumberForm { get; }

    public int Line { get; }

    public int Column { get; }

    public static Json5Token Punctuation(Json5TokenKind kind, int line, int column) =>
        new(kind, string.Empty, Json5NumberForm.Decimal, line, column);

    public static Json5Token EndOfInput(int line, int column) =>
        new(Json5TokenKind.EndOfInput, string.Empty, Json5NumberForm.Decimal, line, column);

    public static Json5Token WithText(Json5TokenKind kind, string text, int line, int column) =>
        new(kind, text, Json5NumberForm.Decimal, line, column);

    public static Json5Token Number(Json5NumberForm form, string normalizedText, int line, int column) =>
        new(Json5TokenKind.NumberLiteral, normalizedText, form, line, column);
}
