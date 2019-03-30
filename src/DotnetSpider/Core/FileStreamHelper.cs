using System.IO;
using System.Text;

namespace DotnetSpider.Core
{
    public static class FileStreamHelper
    {
        public static string ReadAllText(this FileStream fs)
        {
            StringBuilder str = new StringBuilder();
            byte[] b = new byte[fs.Length];
            UTF8Encoding utf = new UTF8Encoding(true);
            while (fs.Read(b, 0, b.Length) > 0)
            {
                str.Append(utf.GetString(b));
            }

            fs.Position = 0;
            return str.ToString();
        }

        public static void WriteAllText(this FileStream fs, string text)
        {
            byte[] codes = new UTF8Encoding(true).GetBytes(text);
            fs.Write(codes, 0, codes.Length);
        }
    }
}