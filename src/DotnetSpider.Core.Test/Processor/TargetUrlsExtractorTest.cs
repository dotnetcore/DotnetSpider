using DotnetSpider.Core.Processor;
using System.Linq;
using Xunit;
using System.Net.Http;

namespace DotnetSpider.Core.Test.Processor
{
	public class TargetUrlsExtractorTest
	{
		[Fact(DisplayName = "RegionAndPatternTargetUrlsExtractor")]
		public void RegionAndPatternTargetUrlsExtractor()
		{
			HttpClient client = new HttpClient();
			var html = client.GetStringAsync("http://www.cnblogs.com").Result;

			var extracotr = new RegionAndPatternTargetUrlsExtractor(".//div[@class='pager']", "/sitehome/p/\\d+", "^http://www\\.cnblogs\\.com/$");

			var site = new Site();
			var page = new Page(new Request("http://cnblogs.com") { Site = site });
			page.Content = html;
			var requets = extracotr.ExtractRequests(page, site).ToList();
			Assert.Equal(12, requets.Count);
			Assert.Contains(requets, r => r.Url == "http://cnblogs.com/sitehome/p/2");
		}
	}
}
