using System;
using DotnetSpider.Core;
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
		public string Encoding { get; set; }

		protected override dynamic FormateValue(dynamic value)
		{
#if !NET_CORE
			return HttpUtility.UrlEncode(value, System.Text.Encoding.GetEncoding(Encoding));
#else
			return WebUtility.UrlEncode(value);
#endif
		}

		protected override void CheckArguments()
		{
			var encoding = System.Text.Encoding.GetEncoding(Encoding);
			if (encoding == null)
			{
				throw new SpiderException($"Can't get encoding: {Encoding}.");
			}
		}
	}
}
