#if !NETSTANDARD
using System.Web;
#else
using System.Net;
#endif
using System;

namespace DotnetSpider.DataFlow.Parser.Formatters
{
	/// <summary>
	/// Converts a text string into a URL-encoded string.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class UrlDecodeFormatter : Formatter
	{
		protected override void CheckArguments()
		{
		}

		/// <summary>
		/// 实现数值的转化
		/// </summary>
		/// <param name="value">数值</param>
		/// <returns>被格式化后的数值</returns>
		protected override string Handle(string value)
		{
			var tmp = value;
#if !NETSTANDARD
			return HttpUtility.UrlDecode(tmp);
#else
			return WebUtility.UrlDecode(tmp);
#endif
		}

	}
}
