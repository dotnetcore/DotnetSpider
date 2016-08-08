using System;
#if !NET_CORE
using System.Web;
#else
using System.Net;
#endif

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property)]
	public class UrlEncodeFormater : Formatter
	{
		public override string Name { get; internal set; } = "UrlEncodeFormater";

		public string Encoding { get; set; }

		public override string Formate(string value)
		{
#if !NET_CORE
			return HttpUtility.UrlEncode(value, System.Text.Encoding.GetEncoding(Encoding));
#else
			return WebUtility.UrlEncode(value);
#endif
		}
	}
}
