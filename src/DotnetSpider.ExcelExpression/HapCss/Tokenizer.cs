using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DotnetSpider.ExcelExpression.HapCss
{
    public class Tokenizer
    {
        public static IEnumerable<Token> GetTokens(string cssFilter)
        {
            var reader = new StringReader(cssFilter);
            while (true)
            {
                var v = reader.Read();

                if (v < 0)
                    yield break;

                var c = (char)v;

                if (c == '>')
                {
                    yield return new Token(">");
                    continue;
                }

                if (c == ' ' || c == '\t')
                    continue;

                var word = c + ReadWord(reader);
                yield return new Token(word);
            }
        }

        private static string ReadWord(StringReader reader)
        {
            var sb = new StringBuilder();
            while (true)
            {
                var v = reader.Read();

                if (v < 0)
                    break;

                var c = (char)v;

                if (c == ' ' || c == '\t')
                    break;

                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
