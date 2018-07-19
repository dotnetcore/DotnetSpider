using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DotnetSpider.HtmlAgilityPack.Css
{
    #region Imports

	

	#endregion

    /// <summary>
    /// Lexer for tokens in CSS selector grammar.
    /// </summary>
    public static class Tokener
    {
        /// <summary>
        /// Parses tokens from a given text source.
        /// </summary>
        public static IEnumerable<Token> Tokenize(TextReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            return Tokenize(reader.ReadToEnd());
        }

        /// <summary>
        /// Parses tokens from a given string.
        /// </summary>
        public static IEnumerable<Token> Tokenize(string input)
        {
            var reader = new Reader(input ?? string.Empty);

            while (reader.Read() != null)
            {
                var ch = reader.Value;

                //
                // Identifier or function
                //
                if (ch == '-' || IsNmStart(ch))
                {
                    reader.Mark();
                    if (reader.Value == '-')
                    {
                        if (!IsNmStart(reader.Read()))
                            throw new FormatException(string.Format("Invalid identifier at position {0}.", reader.Position));
                    }
                    while (IsNmChar(reader.Read())) { /* NOP */ }
                    if (reader.Value == '(')
                        yield return Token.Function(reader.Marked());
                    else
                        yield return Token.Ident(reader.MarkedWithUnread());
                }
                //
                // Integer
                //
                else if (IsDigit(ch))
                {
                    reader.Mark();
                    do { /* NOP */ } while (IsDigit(reader.Read()));
                    yield return Token.Integer(reader.MarkedWithUnread());
                }
                //
                // Whitespace, including that which is coupled with some punctuation
                //
                else if (IsS(ch))
                {
                    var space = ParseWhiteSpace(reader);
                    ch = reader.Read();
                    switch (ch)
                    {
                        case ',': yield return Token.Comma(); break;
                        case ';': yield return Token.Semicolon(); break;
                        case '+': yield return Token.Plus(); break;
                        case '>': yield return Token.Greater(); break;
                        case '~': yield return Token.Tilde(); break;

                        default:
                            reader.Unread();
                            yield return Token.WhiteSpace(space);
                            break;
                    }
                }
                else switch (ch)
                    {
                        case '*': // * or *=
                        case '~': // ~ or ~=
                        case '|': // | or |=
                            {
                                if (reader.Read() == '=')
                                {
                                    yield return ch == '*'
                                               ? Token.SubstringMatch()
                                               : ch == '|' ? Token.DashMatch()
                                               : Token.Includes();
                                }
                                else
                                {
                                    reader.Unread();
                                    yield return ch == '*' || ch == '|'
                                        ? Token.Char(ch.Value)
                                        : Token.Tilde();
                                }
                                break;
                            }
                        case '^': // ^=
                        case '$': // $=
                        case '%': // $=
                        case '!': // !=
                            {
                                if (reader.Read() != '=')
                                    throw new FormatException(string.Format("Invalid character at position {0}.", reader.Position));

                                switch (ch)
                                {
                                    case '^': yield return Token.PrefixMatch(); break;
                                    case '$': yield return Token.SuffixMatch(); break;
                                    case '%': yield return Token.RegexMatch(); break;
                                    case '!': yield return Token.NotEqual(); break;
                                }
                                break;
                            }
                        //
                        // Single-character punctuation
                        //
                        case '.': yield return Token.Dot(); break;
                        case ':': yield return Token.Colon(); break;
                        case ',': yield return Token.Comma(); break;
                        case ';': yield return Token.Semicolon(); break;
                        case '=': yield return Token.Equals(); break;
                        case '[': yield return Token.LeftBracket(); break;
                        case ']': yield return Token.RightBracket(); break;
                        case ')': yield return Token.RightParenthesis(); break;
                        case '+': yield return Token.Plus(); break;
                        case '>': yield return Token.Greater(); break;
                        case '/': yield return Token.Slash(); break;
                        case '#': yield return Token.Hash(ParseHash(reader)); break;
                        //
                        // Single- or double-quoted strings
                        //
                        case '\"':
                        case '\'': yield return ParseString(reader, /* quote */ ch.Value); break;

                        default:
                            throw new FormatException(string.Format("Invalid character at position {0}.", reader.Position));
                    }
            }
            yield return Token.Eoi();
        }

        private static string ParseWhiteSpace(Reader reader)
        {
            Debug.Assert(reader != null);

            reader.Mark();
            while (IsS(reader.Read())) { /* NOP */ }
            return reader.MarkedWithUnread();
        }

        private static string ParseHash(Reader reader)
        {
            Debug.Assert(reader != null);

            reader.MarkFromNext(); // skipping #
            while (IsNmChar(reader.Read())) { /* NOP */ }
            var text = reader.MarkedWithUnread();
            if (text.Length == 0)
                throw new FormatException(string.Format("Invalid hash at position {0}.", reader.Position));
            return text;
        }

        private static Token ParseString(Reader reader, char quote)
        {
            Debug.Assert(reader != null);

            //
            // TODO Support full string syntax!
            //
            // string    {string1}|{string2}
            // string1   \"([^\n\r\f\\"]|\\{nl}|{nonascii}|{escape})*\"
            // string2   \'([^\n\r\f\\']|\\{nl}|{nonascii}|{escape})*\'
            // nonascii  [^\0-\177]
            // escape    {unicode}|\\[^\n\r\f0-9a-f]
            // unicode   \\[0-9a-f]{1,6}(\r\n|[ \n\r\t\f])?
            //

            var strpos = reader.Position;
            reader.MarkFromNext(); // skipping quote

            char? ch;
            StringBuilder sb = null;

            while ((ch = reader.Read()) != quote)
            {
                if (ch == null)
                    throw new FormatException(string.Format("Unterminated string at position {0}.", strpos));

                if (ch == '\\')
                {
                    ch = reader.Read();

                    //
                    // NOTE: Only escaping of quote and backslash supported!
                    //

                    if (ch != quote && ch != '\\')
                        throw new FormatException(string.Format("Invalid escape sequence at position {0} in a string at position {1}.", reader.Position, strpos));

                    if (sb == null)
                        sb = new StringBuilder();

                    sb.Append(reader.MarkedExceptLast());
                    reader.Mark();
                }
            }

            var text = reader.Marked();

            if (sb != null)
                text = sb.Append(text).ToString();

            return Token.String(text);
        }

        private static bool IsDigit(char? ch) // [0-9]
        {
            return ch >= '0' && ch <= '9';
        }

        private static bool IsS(char? ch) // [ \t\r\n\f]
        {
            return ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n' || ch == '\f';
        }

        private static bool IsNmStart(char? ch) // [_a-z]|{nonascii}|{escape}
        {
            return ch == '_'
                || (ch >= 'a' && ch <= 'z')
                || (ch >= 'A' && ch <= 'Z');
        }

        private static bool IsNmChar(char? ch) // [_a-z0-9-]|{nonascii}|{escape}
        {
            return IsNmStart(ch) || ch == '-' || (ch >= '0' && ch <= '9');
        }

        private sealed class Reader
        {
            private readonly string _input;
            private int _index = -1;
            private int _start = -1;

            public Reader(string input)
            {
                _input = input;
            }

            private bool Ready => _index >= 0 && _index < _input.Length;
            public char? Value => Ready ? _input[_index] : (char?)null;
            public int Position => _index + 1;

            public void Mark()
            {
                _start = _index;
            }

            public void MarkFromNext()
            {
                _start = _index + 1;
            }

            public string Marked()
            {
                return Marked(0);
            }

            public string MarkedExceptLast()
            {
                return Marked(-1);
            }

            private string Marked(int trim)
            {
                var start = _start;
                var count = Math.Min(_input.Length, _index + trim) - start;
                return count > 0
                     ? _input.Substring(start, count)
                     : string.Empty;
            }

            public char? Read()
            {
                _index = Position >= _input.Length ? _input.Length : _index + 1;
                return Value;
            }

            public void Unread()
            {
                _index = Math.Max(-1, _index - 1);
            }

            public string MarkedWithUnread()
            {
                var text = Marked();
                Unread();
                return text;
            }
        }
    }
}
