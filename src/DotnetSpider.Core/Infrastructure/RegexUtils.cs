using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Infrastructure
{
	public static class RegexUtil
	{
		public static readonly Regex IpAddress = new Regex(@"((?:(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d))))");
		public static readonly Regex Number = new Regex(@"\d+");
		public static readonly Regex Decimal = new Regex(@"\d+(\.\d+)?");
		public static string Url = @"(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?";
	}
}
