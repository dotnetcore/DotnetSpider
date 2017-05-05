using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Extension.Downloader;
using DotnetSpider.Extension.Model.Attribute;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetSpider.Extension.Test.Downloader
{
	[TestClass]
	public class TargetUrlsCreatorTest
	{
		[TestMethod]
		public void IncrementTargetUrls()
		{
			var spider = new DefaultSpider("test", new Site());
			TestDownloader downloader = new TestDownloader
			{
				DownloadCompleteHandlers = new IDownloadCompleteHandler[]
				{
					new IncrementTargetUrlsCreator("&page=0",2)
				}
			};
			var request = new Request("http://a.com/?&page=0", null);
			Page page = downloader.Download(request, spider);
			var request2 = page.TargetRequests.First();
			Assert.AreEqual("http://a.com/?&page=2", request2.Url.ToString());
			page = downloader.Download(request2, spider);
			request2 = page.TargetRequests.First();
			Assert.AreEqual("http://a.com/?&page=4", request2.Url.ToString());

			downloader = new TestDownloader
			{
				DownloadCompleteHandlers = new IDownloadCompleteHandler[]
				{
					new IncrementTargetUrlsCreator("&page=0",2)
					{
						PageIndexKey = "page_index"
					}
				}
			};

			request = new Request("http://a.com/?&page=0", new Dictionary<string, object>() { { "page_index", 1 } });
			page = downloader.Download(request, spider);
			request2 = page.TargetRequests.First();
			Assert.AreEqual("http://a.com/?&page=2", request2.Url.ToString());
			Assert.AreEqual(2, request2.GetExtra("page_index"));
		}

		public void PaggerStopper()
		{
			var spider = new DefaultSpider("test", new Site());
			TestDownloader downloader = new TestDownloader
			{
				DownloadCompleteHandlers = new IDownloadCompleteHandler[]
				{
					new IncrementTargetUrlsCreator("&page=0",2)
					{
						Stopper = new PaggerStopper
						{
							CurrenctPageSelector = new BaseSelector
							{
								Expression = ""
							}
						}
					}
				}
			};
			var request = new Request("http://a.com/?&page=0", null);
			Page page = downloader.Download(request, spider);
			var request2 = page.TargetRequests.First();
			Assert.AreEqual("http://a.com/?&page=2", request2.Url.ToString());
			page = downloader.Download(request2, spider);
			request2 = page.TargetRequests.First();
			Assert.AreEqual("http://a.com/?&page=4", request2.Url.ToString());
		}
	}
}
