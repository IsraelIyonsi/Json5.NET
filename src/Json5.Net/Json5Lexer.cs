using System.Globalization;
using System.Numerics;
using System.Text;
using static Json5.Json5CharacterClassification;

namespace Json5;

/// <summary>
/// Tokenizes JSON5 text one token at a time, tracking one-based line and column positions
/// for diagnostics. Internal: none of this is public API.
/// </summary>
internal sealed class Json5Lexer
{
    private readonly string _text;
    private int _index;
    private int _line = 1;
    private int _column = 1;
    private Json5Token? _lookahead;

    public Json5Lexer(string text)
    {
        _text = text;
    }

    public Json5Token PeekToken() => _lookahead ??= ReadToken();

    public Json5Token NextToken()
    {
        var token = PeekToken();
        _lookahead = null;
        return token;
    }

    private bool AtEnd => _index >= _text.Length;

    private char? Current => AtEnd ? null : _text[_index];

    private char? PeekAt(int offset) =>
        _index + offset < _text.Length ? _text[_index + offset] : null;

    private char Advance()
    {
        char c = _text[_index];
        _index++;

        if (c == '\r')
        {
            if (!AtEnd && _text[_index] == '\n')
            {
                _index++;
            }

            _line++;
            _column = 1;
        }
        else if (c is '\n' || c == Json5Constants.LineSeparator || c == Json5Constants.ParagraphSeparator)
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }

        return c;
    }

    private Json5Token ReadToken()
    {
        SkipTrivia();

        if (AtEnd)
        {
            return Json5Token.EndOfInput(_line, _column);
        }

        int line = _line;
        int column = _column;
        char c = Current!.Value;

        switch (c)
        {
            case '{': Advance(); return Json5Token.Punctuation(Json5TokenKind.LeftBrace, line, column);
            case '}': Advance(); return Json5Token.Punctuation(Json5TokenKind.RightBrace, line, column);
            case '[': Advance(); return Json5Token.Punctuation(Json5TokenKind.LeftBracket, line, column);
            case ']': Advance(); return Json5Token.Punctuation(Json5TokenKind.RightBracket, line, column);
            case ':': Advance(); return Json5Token.Punctuation(Json5TokenKind.Colon, line, column);
            case ',': Advance(); return Json5Token.Punctuation(Json5TokenKind.Comma, line, column);
            case '"':
            case '\'':
                return ScanString(c);
            case '+':
            case '-':
                return ScanNumber();
            default:
                if (c == '.' || IsDecimalDigit(c))
                {
                    return ScanNumber();
                }

                if (IsIdentifierStart(c) || c == '\\')
                {
                    return ScanIdentifier();
                }

                throw LexError($"Unexpected character '{c}'.", line, column);
        }
    }

    private void SkipTrivia()
    {
        while (!AtEnd)
        {
            char c = Current!.Value;

            if (IsWhitespace(c))
            {
                Advance();
                continue;
            }

            if (c == '/' && PeekAt(1) == '/')
            {
                Advance();
                Advance();
                while (!AtEnd && !IsLineTerminator(Current!.Value))
                {
                    Advance();
                }

                continue;
            }

            if (c == '/' && PeekAt(1) == '*')
            {
                int startLine = _line;
                int startColumn = _column;
                Advance();
                Advance();

                bool closed = false;
                while (!AtEnd)
                {
                    if (Current == '*' && PeekAt(1) == '/')
                    {
                        Advance();
                        Advance();
                        closed = true;
                        break;
                    }

                    Advance();
                }

                if (!closed)
                {
                    throw LexError("Unterminated block comment.", startLine, startColumn);
                }

                continue;
            }

            break;
        }
    }

    private Json5Token ScanString(char quote)
    {
        int startLine = _line;
        int startColumn = _column;
        Advance();

        var sb = new StringBuilder();
        while (true)
        {
            if (AtEnd)
            {
                throw LexError("Unterminated string literal.", startLine, startColumn);
            }

            char c = Current!.Value;

            if (c == quote)
            {
                Advance();
                break;
            }

            if (c == '\\')
            {
                Advance();
                if (AtEnd)
                {
                    throw LexError("Unterminated string literal.", startLine, startColumn);
                }

                AppendEscapeSequence(sb);
                continue;
            }

            if (IsLineTerminator(c))
            {
                throw LexError("Strings cannot contain an unescaped line terminator.", _line, _column);
            }

            sb.Append(c);
            Advance();
        }

        return Json5Token.WithText(Json5TokenKind.StringLiteral, sb.ToString(), startLine, startColumn);
    }

    private void AppendEscapeSequence(StringBuilder sb)
    {
        char e = Current!.Value;

        switch (e)
        {
            case '\'': sb.Append('\''); Advance(); break;
            case '"': sb.Append('"'); Advance(); break;
            case '\\': sb.Append('\\'); Advance(); break;
            case 'b': sb.Append('\b'); Advance(); break;
            case 'f': sb.Append('\f'); Advance(); break;
            case 'n': sb.Append('\n'); Advance(); break;
            case 'r': sb.Append('\r'); Advance(); break;
            case 't': sb.Append('\t'); Advance(); break;
            case 'v': sb.Append('\v'); Advance(); break;
            case '0' when IsDecimalDigit(PeekAt(1)):
                throw LexError("Legacy octal escape sequences are not allowed.");
            case '0':
                sb.Append('\0');
                Advance();
                break;
            case 'x':
                Advance();
                sb.Append(ReadFixedHexEscape(Json5Constants.HexEscapeDigitCount));
                break;
            case 'u':
                Advance();
                sb.Append(ReadFixedHexEscape(Json5Constants.UnicodeEscapeDigitCount));
                break;
            case '\r':
                Advance();
                if (Current == '\n')
                {
                    Advance();
                }

                break;
            case '\n':
            case Json5Constants.LineSeparator:
            case Json5Constants.ParagraphSeparator:
                Advance();
                break;
            default:
                if (IsDecimalDigit(e))
                {
                    throw LexError("Digits 1-9 are not allowed as escape characters.");
                }

                sb.Append(e);
                Advance();
                break;
        }
    }

    private char ReadFixedHexEscape(int digitCount)
    {
        int value = 0;
        for (int i = 0; i < digitCount; i++)
        {
            if (AtEnd || !IsHexDigit(Current))
            {
                throw LexError($"Expected {digitCount} hexadecimal digits.");
            }

            value = (value * Json5Constants.HexRadix) + HexDigitValue(Current!.Value);
            Advance();
        }

        return (char)value;
    }

    private Json5Token ScanIdentifier()
    {
        int startLine = _line;
        int startColumn = _column;
        var sb = new StringBuilder();

        AppendIdentifierChar(sb, isStart: true);
        while (!AtEnd && (Current == '\\' || IsIdentifierPart(Current!.Value)))
        {
            AppendIdentifierChar(sb, isStart: false);
        }

        return Json5Token.WithText(Json5TokenKind.Identifier, sb.ToString(), startLine, startColumn);
    }

    private void AppendIdentifierChar(StringBuilder sb, bool isStart)
    {
        if (Current == '\\')
        {
            Advance();
            if (Current != 'u')
            {
                throw LexError("Expected a unicode escape sequence.");
            }

            Advance();
            char resolved = ReadFixedHexEscape(Json5Constants.UnicodeEscapeDigitCount);
            bool valid = isStart ? IsIdentifierStart(resolved) : IsIdentifierPart(resolved);
            if (!valid)
            {
                throw LexError("The escaped character is not valid at this position in an identifier.");
            }

            sb.Append(resolved);
            return;
        }

        sb.Append(Current!.Value);
        Advance();
    }

    private Json5Token ScanNumber()
    {
        int startLine = _line;
        int startColumn = _column;
        bool negative = false;

        if (Current == '+')
        {
            Advance();
        }
        else if (Current == '-')
        {
            negative = true;
            Advance();
        }

        if (TryConsumeKeyword(Json5Constants.InfinityKeyword))
        {
            var form = negative ? Json5NumberForm.NegativeInfinity : Json5NumberForm.PositiveInfinity;
            return Json5Token.Number(form, string.Empty, startLine, startColumn);
        }

        if (TryConsumeKeyword(Json5Constants.NanKeyword))
        {
            return Json5Token.Number(Json5NumberForm.NaN, string.Empty, startLine, startColumn);
        }

        string sign = negative ? "-" : string.Empty;

        if (Current == '0' && (PeekAt(1) == 'x' || PeekAt(1) == 'X'))
        {
            Advance();
            Advance();
            return Json5Token.Number(Json5NumberForm.Decimal, ScanHexInteger(sign), startLine, startColumn);
        }

        var norm = new StringBuilder(sign);
        ScanDecimalLiteral(norm);
        return Json5Token.Number(Json5NumberForm.Decimal, norm.ToString(), startLine, startColumn);
    }

    private string ScanHexInteger(string sign)
    {
        int hexStart = _index;
        while (IsHexDigit(Current))
        {
            Advance();
        }

        if (_index == hexStart)
        {
            throw LexError("Expected a hexadecimal digit after '0x'.");
        }

        string hexDigits = _text[hexStart.._index];
        var value = BigInteger.Parse("0" + hexDigits, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return sign + value.ToString(CultureInfo.InvariantCulture);
    }

    private void ScanDecimalLiteral(StringBuilder norm)
    {
        bool hasIntegerPart;

        if (Current == '.')
        {
            hasIntegerPart = false;
            norm.Append('0');
        }
        else if (Current == '0')
        {
            hasIntegerPart = true;
            norm.Append('0');
            Advance();
            if (IsDecimalDigit(Current))
            {
                throw LexError("A number with a leading zero must not be followed by another digit.");
            }
        }
        else if (IsDecimalDigit(Current))
        {
            hasIntegerPart = true;
            while (IsDecimalDigit(Current))
            {
                norm.Append(Current!.Value);
                Advance();
            }
        }
        else
        {
            throw LexError("Expected a digit.");
        }

        if (Current == '.')
        {
            Advance();
            norm.Append('.');
            int fracStart = norm.Length;
            while (IsDecimalDigit(Current))
            {
                norm.Append(Current!.Value);
                Advance();
            }

            if (norm.Length == fracStart)
            {
                if (!hasIntegerPart)
                {
                    throw LexError("Expected a digit after the decimal point.");
                }

                norm.Append('0');
            }
        }

        if (Current is 'e' or 'E')
        {
            Advance();
            norm.Append('e');
            if (Current is '+' or '-')
            {
                norm.Append(Current!.Value);
                Advance();
            }

            int expStart = norm.Length;
            while (IsDecimalDigit(Current))
            {
                norm.Append(Current!.Value);
                Advance();
            }

            if (norm.Length == expStart)
            {
                throw LexError("Expected a digit in the exponent.");
            }
        }
    }

    private bool TryConsumeKeyword(string keyword)
    {
        if (_index + keyword.Length > _text.Length)
        {
            return false;
        }

        if (string.CompareOrdinal(_text, _index, keyword, 0, keyword.Length) != 0)
        {
            return false;
        }

        char? boundary = _index + keyword.Length < _text.Length ? _text[_index + keyword.Length] : null;
        if (boundary is not null && IsIdentifierPart(boundary.Value))
        {
            return false;
        }

        for (int i = 0; i < keyword.Length; i++)
        {
            Advance();
        }

        return true;
    }

    private Json5Exception LexError(string message) => new(message, _line, _column);

    private static Json5Exception LexError(string message, int line, int column) => new(message, line, column);
}
