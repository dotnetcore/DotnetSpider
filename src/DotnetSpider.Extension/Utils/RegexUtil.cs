using System.Text.RegularExpressions;

namespace DotnetSpider.Extension.Utils
{
	public class RegexUtil
	{
		public static Regex StringTypeRegex = new Regex(@"string\(\d+\)");
		public static Regex BoolTypeRegex = new Regex(@"bool");
		public static Regex NumRegex = new Regex(@"\d+");
	}
}
