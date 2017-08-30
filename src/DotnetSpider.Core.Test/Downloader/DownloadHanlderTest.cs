using DotnetSpider.Core.Downloader;
using Xunit;

namespace DotnetSpider.Core.Test.Downloader
{

	public class DownloadHanlderTest
	{
		[Fact]
		public void RetryWhenContainsIllegalStringHandler()
		{
			var spider = new DefaultSpider("test", new Site());

			TestDownloader downloader1 = new TestDownloader();
			downloader1.AddAfterDownloadCompleteHandler(new RetryWhenContainsContentHandler
			{
				Content = "aabbcccdefg下载人数100"
			});
			var request1 = new Request("http://www.163.com/", null);
			Page page = downloader1.Download(request1, spider);
			Assert.Single(page.TargetRequests);

			downloader1 = new TestDownloader();
			downloader1.AddAfterDownloadCompleteHandler(new RetryWhenContainsContentHandler
			{
				Content = "网易倒闭啦"
			});
			page = downloader1.Download(request1, spider);
			Assert.Empty(page.TargetRequests);
		}
	}
}
