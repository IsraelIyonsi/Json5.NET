using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json5;

/// <summary>
/// Parses JSON5 (json5.org) text, the JSON superset that adds comments, trailing commas,
/// unquoted and single-quoted object keys, single-quoted strings, string line continuations,
/// leading/trailing decimal points, hexadecimal numbers and signed <c>Infinity</c>/<c>NaN</c>,
/// into <see cref="System.Text.Json"/> values.
/// </summary>
public static class Json5
{
    /// <summary>
    /// Parses JSON5 text into a <see cref="JsonNode"/> tree.
    /// </summary>
    /// <param name="json5Text">The JSON5 document to parse.</param>
    /// <returns>
    /// The parsed value. This is <see langword="null"/> when the document's single top-level
    /// value is the JSON5 <c>null</c> literal.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="json5Text"/> is <see langword="null"/>.</exception>
    /// <exception cref="Json5Exception"><paramref name="json5Text"/> is not valid JSON5.</exception>
    public static JsonNode? Parse(string json5Text)
    {
        ArgumentNullException.ThrowIfNull(json5Text);
        return new Json5Parser(json5Text).Parse();
    }

    /// <summary>
    /// Attempts to parse JSON5 text into a <see cref="JsonNode"/> tree without throwing on
    /// malformed input.
    /// </summary>
    /// <param name="json5Text">The JSON5 document to parse, or <see langword="null"/>.</param>
    /// <param name="result">
    /// The parsed value on success, or <see langword="null"/> on failure. Because JSON5's own
    /// <c>null</c> literal also parses to <see langword="null"/>, a <see langword="null"/>
    /// <paramref name="result"/> does not by itself indicate failure; check the return value.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="json5Text"/> is valid JSON5; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? json5Text, out JsonNode? result)
    {
        if (json5Text is null)
        {
            result = null;
            return false;
        }

        try
        {
            result = new Json5Parser(json5Text).Parse();
            return true;
        }
        catch (Json5Exception)
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Parses JSON5 text and deserializes it into an instance of <typeparamref name="T"/>,
    /// via <see cref="JsonSerializer"/>. Numbers that came from a JSON5 <c>Infinity</c> or
    /// <c>NaN</c> literal deserialize correctly into floating-point members: this method always
    /// enables <see cref="JsonNumberHandling.AllowNamedFloatingPointLiterals"/>, in addition to
    /// whatever <paramref name="options"/> already requests, since System.Text.Json otherwise
    /// has no way to carry those values through its own JSON text representation.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON5 document into.</typeparam>
    /// <param name="json5Text">The JSON5 document to parse.</param>
    /// <param name="options">Options that control deserialization, or <see langword="null"/> for the defaults.</param>
    /// <returns>The deserialized value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json5Text"/> is <see langword="null"/>.</exception>
    /// <exception cref="Json5Exception"><paramref name="json5Text"/> is not valid JSON5.</exception>
    public static T? Deserialize<T>(string json5Text, JsonSerializerOptions? options = null)
    {
        var node = Parse(json5Text);
        return JsonSerializer.Deserialize<T>(node, WithNamedFloatingPointLiterals(options));
    }

    private static JsonSerializerOptions WithNamedFloatingPointLiterals(JsonSerializerOptions? options)
    {
        if (options is null)
        {
            return DefaultDeserializeOptions;
        }

        if (options.NumberHandling.HasFlag(JsonNumberHandling.AllowNamedFloatingPointLiterals))
        {
            return options;
        }

        // Cache the augmented clone per caller-supplied options instance so repeated
        // Deserialize<T> calls with the same options reuse System.Text.Json's cached type
        // metadata instead of paying for a fresh JsonSerializerOptions (and its metadata cache)
        // on every call.
        return AugmentedOptionsCache.GetValue(options, static o => new JsonSerializerOptions(o)
        {
            NumberHandling = o.NumberHandling | JsonNumberHandling.AllowNamedFloatingPointLiterals,
        });
    }

    private static readonly JsonSerializerOptions DefaultDeserializeOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
    };

    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> AugmentedOptionsCache = new();
}
