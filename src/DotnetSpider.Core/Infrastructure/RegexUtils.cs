using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Infrastructure
{
	public class RegexUtil
	{
		public static Regex IpAddressRegex = new Regex(@"((?:(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d))))");
		public static Regex StringTypeRegex = new Regex(@"string\(\d+\)");
		public static Regex BoolTypeRegex = new Regex(@"bool");
		public static Regex NumRegex = new Regex(@"\d+");
		public static Regex DecimalRegex = new Regex(@"\d+(\.\d+)?");
	}
}
