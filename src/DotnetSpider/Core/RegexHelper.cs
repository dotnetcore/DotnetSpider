using System.Text.RegularExpressions;

namespace DotnetSpider.Core
{
    public class RegexHelper
    {
        /// <summary>
        /// IP正则表达式
        /// </summary>
        public static readonly Regex IpAddress = new Regex(@"((?:(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d))))");
        /// <summary>
        /// 数字正则表达式
        /// </summary>
        public static readonly Regex Number = new Regex(@"\d+");
        /// <summary>
        /// 小数正则表达式
        /// </summary>
        public static readonly Regex Decimal = new Regex(@"\d+(\.\d+)?");
        /// <summary>
        /// URL正则表达式
        /// </summary>
        public static string Url = @"(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?";
    }
}