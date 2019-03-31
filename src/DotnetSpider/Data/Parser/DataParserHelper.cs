using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using DotnetSpider.Downloader;

namespace DotnetSpider.Data.Parser
{
	public static class DataParserHelper
	{
		public static Func<DataFlowContext, List<string>> QueryFollowRequestsByXPath(params string[] xPaths)
		{
			return context =>
			{
				var urls = new List<string>();
				foreach (var xpath in xPaths)
				{
					var links = context.GetSelectable().XPath(xpath).Links().GetValues();
					foreach (var link in links)
					{
#if !NETSTANDARD
                        urls.Add(System.Web.HttpUtility.HtmlDecode(System.Web.HttpUtility.UrlDecode(link)));
#else
						urls.Add(WebUtility.HtmlDecode(WebUtility.UrlDecode(link)));
#endif
					}
				}

				return urls;
			};
		}

		public static Func<Request, bool> CanParseByRegex(params string[] patterns)
		{
			return request =>
			{
				foreach (var pattern in patterns)
				{
					if (Regex.IsMatch(request.Url, pattern))
					{
						return true;
					}
				}

				return false;
			};
		}
	}
}