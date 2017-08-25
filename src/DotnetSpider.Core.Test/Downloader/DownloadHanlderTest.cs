using DotnetSpider.Core.Downloader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetSpider.Core.Test.Downloader
{
	[TestClass]
	public class DownloadHanlderTest
	{
		[TestMethod]
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
			Assert.AreEqual(1, page.TargetRequests.Count);

			downloader1 = new TestDownloader();
			downloader1.AddAfterDownloadCompleteHandler(new RetryWhenContainsContentHandler
			{
				Content = "网易倒闭啦"
			});
			page = downloader1.Download(request1, spider);
			Assert.AreEqual(0, page.TargetRequests.Count);
		}
	}
}
