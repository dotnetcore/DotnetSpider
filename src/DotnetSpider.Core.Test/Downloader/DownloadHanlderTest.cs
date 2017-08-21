using System.Collections.Generic;
using DotnetSpider.Core.Downloader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetSpider.Core.Test.Downloader
{
	[TestClass]
	public class DownloadHanlderTest
	{
		[TestMethod]
		public void SubContentHandler()
		{
			var spider = new DefaultSpider("test", new Site());
			TestDownloader downloader1 = new TestDownloader();
			downloader1.AddAfterDownloadCompleteHandler(new SubContentHandler
			{
				Start = "a",
				End = "c"
			});
			var request1 = new Request("http://a.com/", null);
			Page page = downloader1.Download(request1, spider);
			Assert.AreEqual("aabbc", page.Content);

			downloader1 = new TestDownloader();
			downloader1.AddAfterDownloadCompleteHandler(new SubContentHandler
			{
				Start = "a",
				End = "c",
				EndOffset = 1
			});

			page = downloader1.Download(request1, spider);
			Assert.AreEqual("aabb", page.Content);

			downloader1 = new TestDownloader();
			downloader1.AddAfterDownloadCompleteHandler(new SubContentHandler
			{
				Start = "a",
				End = "c",
				StartOffset = 1
			});
			page = downloader1.Download(request1, spider);
			Assert.AreEqual("abbc", page.Content);

			downloader1 = new TestDownloader();
			downloader1.AddAfterDownloadCompleteHandler(new SubContentHandler
			{
				Start = "a",
				End = "c",
				StartOffset = 1,
				EndOffset = 1
			});

			page = downloader1.Download(request1, spider);
			Assert.AreEqual("abb", page.Content);

			downloader1 = new TestDownloader();
			downloader1.AddAfterDownloadCompleteHandler(new SubContentHandler
			{
				Start = "a",
				End = "c",
				StartOffset = 10
			});

			var downloader2 = downloader1;

			try
			{
				page = downloader2.Download(request1, spider);
				throw new System.Exception("test failed.");
			}
			catch (SpiderException exception)
			{
				Assert.AreEqual("Sub content failed. Please check your settings.", exception.Message);
			}

			downloader1 = new TestDownloader();
			downloader1.AddAfterDownloadCompleteHandler(new SubContentHandler
			{
				Start = "a",
				End = "c",
				EndOffset = 20
			});

			try
			{
				page = downloader1.Download(request1, spider);
				throw new System.Exception("test failed.");
			}
			catch (SpiderException exception)
			{
				Assert.AreEqual("Sub content failed. Please check your settings.", exception.Message);
			}
		}

		[TestMethod]
		public void RetryWhenContainsIllegalStringHandler()
		{
			var spider = new DefaultSpider("test", new Site());

			TestDownloader downloader1 = new TestDownloader();
			downloader1.AddAfterDownloadCompleteHandler(new RetryWhenContainsHandler
			{
				Content = "aabbcccdefg下载人数100"
			});
			var request1 = new Request("http://www.163.com/", null);
			Page page = downloader1.Download(request1, spider);
			Assert.AreEqual(1, page.TargetRequests.Count);

			downloader1 = new TestDownloader();
			downloader1.AddAfterDownloadCompleteHandler(new RetryWhenContainsHandler
			{
				Content = "网易倒闭啦"
			});
			page = downloader1.Download(request1, spider);
			Assert.AreEqual(0, page.TargetRequests.Count);
		}
	}
}
