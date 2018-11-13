using System;
#if !NETSTANDARD
using System.Web;
#else
using System.Net;
#endif

namespace DotnetSpider.Extraction.Model.Formatter
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
		protected override object FormatValue(object value)
		{
			string tmp = value.ToString();
#if !NETSTANDARD
			return HttpUtility.UrlDecode(tmp);
#else
			return WebUtility.UrlDecode(tmp);
#endif
		}

	}
}
