using System.Linq;
using System.Net.Http;
using DotnetSpider.Common;
using DotnetSpider.Core.Processor.TargetRequestExtractors;
using Xunit;
using DotnetSpider.Downloader;

namespace DotnetSpider.Core.Test
{
	public class TargetRequestExtractorTest
	{
		[Fact(DisplayName = "RegionAndPatternTargetUrlsExtractor")]
		public void RegionAndPatternTargetUrlsExtractor()
		{
			HttpClient client = new HttpClient();
			var html = client.GetStringAsync("http://www.cnblogs.com").Result;

			var extracotr = new RegionAndPatternTargetRequestExtractor(".//div[@class='pager']", "/sitehome/p/\\d+", "^http://www\\.cnblogs\\.com/$");
			var page = new Page(new Request("http://cnblogs.com"));
			page.Content = html;
			page.ContentType = ContentType.Html;
			var requets = Enumerable.ToList(extracotr.ExtractRequests(page));
			Assert.Equal(11, requets.Count);
			Assert.Contains(requets, r => r.Url == "http://cnblogs.com/sitehome/p/2");
		}
	}
}
