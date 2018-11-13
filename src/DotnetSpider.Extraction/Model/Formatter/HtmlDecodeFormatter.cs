using System;
#if !NETSTANDARD
using System.Web;
#else
using System.Net;
#endif

namespace DotnetSpider.Extraction.Model.Formatter
{
	/// <summary>
	/// 把数值进行HTML解码
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class HtmlDecodeFormatter : Formatter
	{
		/// <summary>
		/// 实现数值的转化
		/// </summary>
		/// <param name="value">数值</param>
		/// <returns>被格式化后的数值</returns>
		protected override object FormatValue(object value)
		{
			string tmp = value.ToString();
#if !NETSTANDARD
			return HttpUtility.HtmlDecode(tmp);
#else
			return WebUtility.HtmlDecode(tmp);
#endif
		}

		/// <summary>
		/// 校验参数是否设置正确
		/// </summary>
		protected override void CheckArguments()
		{
		}
	}
}
