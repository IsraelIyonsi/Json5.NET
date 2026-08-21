using System.Text.Json;

namespace Json5;

/// <summary>
/// Thrown when text passed to <see cref="Json5Convert"/> is not valid JSON5. Carries the one-based
/// line and column of the character where parsing failed.
/// </summary>
public sealed class Json5Exception : JsonException
{
    /// <summary>Initializes the exception with a message and the one-based source position where parsing failed.</summary>
    /// <param name="message">Description of the JSON5 grammar rule that was violated.</param>
    /// <param name="line">The one-based line number of the character where parsing failed.</param>
    /// <param name="column">The one-based column number of the character where parsing failed.</param>
    public Json5Exception(string message, int line, int column)
        : base(FormatMessage(message, line, column))
    {
        Line = line;
        Column = column;
    }

    /// <summary>The one-based line number of the character where parsing failed.</summary>
    public int Line { get; }

    /// <summary>
    /// The one-based column number of the character where parsing failed, counted in UTF-16
    /// code units. A character outside the Basic Multilingual Plane (represented as a surrogate
    /// pair) advances the column by 2, not 1.
    /// </summary>
    public int Column { get; }

    private static string FormatMessage(string message, int line, int column) =>
        $"{message} (line {line}, column {column})";
}
