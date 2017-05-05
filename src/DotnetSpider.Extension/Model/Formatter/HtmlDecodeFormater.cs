using System;
#if !NET_CORE
using System.Web;
#else
using System.Net;
#endif

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property)]
	public class HtmlDecodeFormater : Formatter
	{
		protected override dynamic FormateValue(dynamic value)
		{
			string tmp = value.ToString();
#if !NET_CORE
			return HttpUtility.HtmlDecode(tmp);
#else
			return WebUtility.HtmlDecode(tmp);
#endif
		}

		protected override void CheckArguments()
		{
		}
	}
}
