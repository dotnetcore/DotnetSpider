using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Extension.Downloader;
using DotnetSpider.Extension.Model.Attribute;
using Xunit;

namespace DotnetSpider.Test.Downloader
{
	public class TargetUrlsCreatorTest
	{
		[Fact]
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
			var request = new Request("http://a.com/?&page=0", 0, null);
			Page page = downloader.Download(request, spider);
			var request2 = page.TargetRequests.First();
			Assert.Equal("http://a.com/?&page=2", request2.Url.ToString());
			page = downloader.Download(request2, spider);
			request2 = page.TargetRequests.First();
			Assert.Equal("http://a.com/?&page=4", request2.Url.ToString());
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
			var request = new Request("http://a.com/?&page=0", 0, null);
			Page page = downloader.Download(request, spider);
			var request2 = page.TargetRequests.First();
			Assert.Equal("http://a.com/?&page=2", request2.Url.ToString());
			page = downloader.Download(request2, spider);
			request2 = page.TargetRequests.First();
			Assert.Equal("http://a.com/?&page=4", request2.Url.ToString());
		}
	}
}
