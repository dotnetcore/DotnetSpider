using System.Collections.Generic;
using System;

namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// A simple PageProcessor.
	/// </summary>
	public class SimplePageProcessor : BasePageProcessor
	{
		private readonly string _urlPattern;

		public SimplePageProcessor(string startUrl, string urlPattern)
		{
			Site = new Site();
			Site.AddStartUrl(startUrl);
			Uri url = new Uri(startUrl);
			Site.Domain = url.Host;
			//compile "*" expression to regex
			_urlPattern = "(" + urlPattern.Replace(".", "\\.").Replace("*", "[^\"'#]*") + ")";

			// 指定目标URL的筛选条件
			TargetUrlRegions.Add(Selector.Selectors.XPath(".//a/@href"));
		}

		protected override void Handle(Page page)
		{
			//extract by XPath
			page.AddResultItem("title", page.Selectable.XPath("//title"));
			page.AddResultItem("html", page.Selectable.ToString());
			//extract by Readability
			page.AddResultItem("content", page.Selectable.SmartContent());
		}
	}
}
