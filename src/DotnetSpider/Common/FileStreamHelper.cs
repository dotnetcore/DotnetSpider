using System.IO;
using System.Text;

namespace DotnetSpider.Common
{
    public static class FileStreamHelper
    {
        public static string ReadAllText(this FileStream fs)
        {
            var str = new StringBuilder();
            var b = new byte[fs.Length];
            var utf = new UTF8Encoding(true);
            while (fs.Read(b, 0, b.Length) > 0)
            {
                str.Append(utf.GetString(b));
            }

            fs.Position = 0;
            return str.ToString();
        }

        public static void WriteAllText(this FileStream fs, string text)
        {
            var codes = new UTF8Encoding(true).GetBytes(text);
            fs.Write(codes, 0, codes.Length);
        }
    }
}