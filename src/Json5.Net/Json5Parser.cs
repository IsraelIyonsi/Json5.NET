using System.Diagnostics;
using System.Text.Json.Nodes;

namespace Json5;

/// <summary>
/// Recursive-descent parser that turns a JSON5 token stream into a <see cref="JsonNode"/> tree.
/// Internal: none of this is public API.
/// </summary>
internal sealed class Json5Parser
{
    private readonly Json5Lexer _lexer;
    private int _depth;

    public Json5Parser(string text)
    {
        _lexer = new Json5Lexer(text);
    }

    public JsonNode? Parse()
    {
        var value = ParseValue();
        var trailing = _lexer.NextToken();
        if (trailing.Kind != Json5TokenKind.EndOfInput)
        {
            throw Error(trailing, "Unexpected content after the top-level value.");
        }

        return value;
    }

    private JsonNode? ParseValue()
    {
        var token = _lexer.NextToken();
        return token.Kind switch
        {
            Json5TokenKind.LeftBrace => ParseObject(token),
            Json5TokenKind.LeftBracket => ParseArray(token),
            Json5TokenKind.StringLiteral => JsonValue.Create(token.Text),
            Json5TokenKind.NumberLiteral => CreateNumber(token),
            Json5TokenKind.Identifier => ParseIdentifierValue(token),
            _ => throw Error(token, "Expected a value."),
        };
    }

    private JsonNode? ParseIdentifierValue(Json5Token token) => token.Text switch
    {
        Json5Constants.TrueKeyword => JsonValue.Create(true),
        Json5Constants.FalseKeyword => JsonValue.Create(false),
        Json5Constants.NullKeyword => null,
        Json5Constants.InfinityKeyword => JsonValue.Create(double.PositiveInfinity),
        Json5Constants.NanKeyword => JsonValue.Create(double.NaN),
        _ => throw Error(token, $"Unexpected identifier '{token.Text}'; expected a value."),
    };

    private static JsonNode CreateNumber(Json5Token token) => token.NumberForm switch
    {
        Json5NumberForm.Decimal => JsonNode.Parse(token.Text)!,
        Json5NumberForm.PositiveInfinity => JsonValue.Create(double.PositiveInfinity),
        Json5NumberForm.NegativeInfinity => JsonValue.Create(double.NegativeInfinity),
        Json5NumberForm.NaN => JsonValue.Create(double.NaN),
        _ => throw new UnreachableException(),
    };

    private JsonObject ParseObject(Json5Token openToken)
    {
        EnterScope(openToken);
        var result = new JsonObject();

        if (_lexer.PeekToken().Kind == Json5TokenKind.RightBrace)
        {
            _lexer.NextToken();
            ExitScope();
            return result;
        }

        while (true)
        {
            var nameToken = _lexer.NextToken();
            string name = nameToken.Kind switch
            {
                Json5TokenKind.StringLiteral => nameToken.Text,
                Json5TokenKind.Identifier => nameToken.Text,
                _ => throw Error(nameToken, "Expected a property name or '}'."),
            };

            var colonToken = _lexer.NextToken();
            if (colonToken.Kind != Json5TokenKind.Colon)
            {
                throw Error(colonToken, "Expected ':' after the property name.");
            }

            result[name] = ParseValue();

            var separator = _lexer.NextToken();
            if (separator.Kind == Json5TokenKind.Comma)
            {
                if (_lexer.PeekToken().Kind == Json5TokenKind.RightBrace)
                {
                    _lexer.NextToken();
                    break;
                }

                continue;
            }

            if (separator.Kind == Json5TokenKind.RightBrace)
            {
                break;
            }

            throw Error(separator, "Expected ',' or '}'.");
        }

        ExitScope();
        return result;
    }

    private JsonArray ParseArray(Json5Token openToken)
    {
        EnterScope(openToken);
        var result = new JsonArray();

        if (_lexer.PeekToken().Kind == Json5TokenKind.RightBracket)
        {
            _lexer.NextToken();
            ExitScope();
            return result;
        }

        while (true)
        {
            result.Add(ParseValue());

            var separator = _lexer.NextToken();
            if (separator.Kind == Json5TokenKind.Comma)
            {
                if (_lexer.PeekToken().Kind == Json5TokenKind.RightBracket)
                {
                    _lexer.NextToken();
                    break;
                }

                continue;
            }

            if (separator.Kind == Json5TokenKind.RightBracket)
            {
                break;
            }

            throw Error(separator, "Expected ',' or ']'.");
        }

        ExitScope();
        return result;
    }

    private void EnterScope(Json5Token openToken)
    {
        _depth++;
        if (_depth > Json5Constants.MaxNestingDepth)
        {
            throw Error(openToken, $"Maximum nesting depth of {Json5Constants.MaxNestingDepth} exceeded.");
        }
    }

    private void ExitScope() => _depth--;

    private static Json5Exception Error(Json5Token token, string message) => new(message, token.Line, token.Column);
}
