using DotnetSpider.Core.Downloader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetSpider.Core.Test.Downloader
{
	[TestClass]
	public class TestDownloader : BaseDownloader
	{
		protected override Page DowloadContent(Request request, ISpider spider)
		{
			var site = spider.Site;
			return new Page(request, ContentType.Html, site.RemoveOutboundLinks ? site.Domains : null)
			{
				Content = "aabbcccdefg下载人数100"
			};
		}
	}
}
