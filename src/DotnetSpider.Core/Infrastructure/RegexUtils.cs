using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Infrastructure
{
	public static class RegexUtil
	{
		public static readonly Regex IpAddressRegex = new Regex(@"((?:(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d))))");
		public static readonly Regex StringTypeRegex = new Regex(@"string\(\d+\)");
		public static readonly Regex BoolTypeRegex = new Regex(@"bool");
		public static readonly Regex NumRegex = new Regex(@"\d+");
		public static readonly Regex DecimalRegex = new Regex(@"\d+(\.\d+)?");
        public static  readonly string UrlRegex = @"(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?";

    }
}
