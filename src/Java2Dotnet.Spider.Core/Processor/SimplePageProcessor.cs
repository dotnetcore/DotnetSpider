using System.Collections.Generic;
using System;

namespace Java2Dotnet.Spider.Core.Processor
{
	/// <summary>
	/// A simple PageProcessor.
	/// </summary>
	public class SimplePageProcessor : IPageProcessor
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
		}

		public void Process(Page page)
		{
			List<string> requests = page.Selectable.XPath(".//a/@href").Regex(_urlPattern).GetValue();
			//add urls to fetch
			page.AddTargetRequests(requests);
			//extract by XPath
			page.AddResultItem("title", page.Selectable.XPath("//title"));
			page.AddResultItem("html", page.Selectable.ToString());
			//extract by Readability
			page.AddResultItem("content", page.Selectable.SmartContent());
		}

		/// <summary>
		/// Get the site settings
		/// </summary>
		public Site Site { get; set; }
	}
}
