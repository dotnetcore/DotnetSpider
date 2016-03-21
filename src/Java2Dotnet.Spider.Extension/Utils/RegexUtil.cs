using System.Text.RegularExpressions;

namespace Java2Dotnet.Spider.Extension.Utils
{
	public class RegexUtil
	{
		public static Regex StringTypeRegex = new Regex(@"string\(\d+\)");
		public static Regex IntTypeRegex = new Regex(@"(^int\(\d+\)$|^int$)");
		public static Regex BigIntTypeRegex = new Regex(@"(^bigint\(\d+\)$|^bigint$)");
		public static Regex FloatTypeRegex = new Regex(@"(float\(\d+\)|float)");
		public static Regex DoubleTypeRegex = new Regex(@"(double\(\d+\)|double)");
		public static Regex DateTypeRegex = new Regex(@"(date)");
		public static Regex TimeStampTypeRegex = new Regex(@"(timestamp\(\d+\)|timestamp)");
		public static Regex NumRegex = new Regex(@"\d+");
	}
}
