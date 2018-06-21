using System.Linq;
using DotnetSpider.Core;
using Xunit;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Processor;

namespace DotnetSpider.Extension.Test.Downloader
{

	public class TargetUrlsCreatorTest
	{
		[Fact(DisplayName = "TargetUrlsCreator_IncrementTargetUrls")]
		public void IncrementTargetUrls()
		{
			var spider = new DefaultSpider("test", new Site());
			TestDownloader downloader = new TestDownloader();

			downloader.AddAfterDownloadCompleteHandler(new TargetUrlsHandler(new AutoIncrementTargetUrlsExtractor("&page=0", 2)));

			var request = new Request("http://a.com/?&page=0", null);
			Page page = downloader.Download(request, spider).Result;
			var request2 = page.TargetRequests.First();
			Assert.Equal("http://a.com/?&page=2", request2.Url.ToString());
			page = downloader.Download(request2, spider).Result;
			request2 = page.TargetRequests.First();
			Assert.Equal("http://a.com/?&page=4", request2.Url.ToString());
		}
	}
}
