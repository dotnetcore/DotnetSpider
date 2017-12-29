using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Core;
using Xunit;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Processor;

namespace DotnetSpider.Extension.Test.Downloader
{

	public class TargetUrlsCreatorTest
	{
		[Fact]
		public void IncrementTargetUrls()
		{
			var spider = new DefaultSpider("test", new Site());
			TestDownloader downloader = new TestDownloader();

			downloader.AddAfterDownloadCompleteHandler(new TargetUrlsHandler(new AutoIncrementTargetUrlsExtractor("&page=0", 2)));

			var request = new Request("http://a.com/?&page=0", null);
			Page page = downloader.Download(request, spider);
			var request2 = page.TargetRequests.First();
			Assert.Equal("http://a.com/?&page=2", request2.Url.ToString());
			page = downloader.Download(request2, spider);
			request2 = page.TargetRequests.First();
			Assert.Equal("http://a.com/?&page=4", request2.Url.ToString());

			downloader = new TestDownloader();
			downloader.AddAfterDownloadCompleteHandler(new TargetUrlsHandler(new RequestExtraTargetUrlsBuilder("&page=0", "page_index")));

			request = new Request("http://a.com/?&page=0", new Dictionary<string, object>() { { "page_index", 2 } });
			request.Site = new Site();
			page = downloader.Download(request, spider);
			request2 = page.TargetRequests.First();
			Assert.Equal("http://a.com/?&page=2", request2.Url.ToString());
			Assert.Equal(2, request2.GetExtra("page_index"));
		}
	}
}
