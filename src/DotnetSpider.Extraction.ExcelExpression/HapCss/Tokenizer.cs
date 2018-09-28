using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Extraction.ExcelExpression.HapCss
{
    public class Tokenizer
    {
        public static IEnumerable<Token> GetTokens(string cssFilter)
        {
            var reader = new System.IO.StringReader(cssFilter);
            while (true)
            {
                int v = reader.Read();

                if (v < 0)
                    yield break;

                char c = (char)v;

                if (c == '>')
                {
                    yield return new Token(">");
                    continue;
                }

                if (c == ' ' || c == '\t')
                    continue;

                string word = c + ReadWord(reader);
                yield return new Token(word);
            }
        }

        private static string ReadWord(System.IO.StringReader reader)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                int v = reader.Read();

                if (v < 0)
                    break;

                char c = (char)v;

                if (c == ' ' || c == '\t')
                    break;

                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
