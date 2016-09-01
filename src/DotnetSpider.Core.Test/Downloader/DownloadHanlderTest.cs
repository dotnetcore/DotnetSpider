using System.Collections.Generic;
using DotnetSpider.Core.Downloader;
using Xunit;

namespace DotnetSpider.Core.Test.Downloader
{
	public class DownloadHanlderTest
	{
		[Fact]
		public void GeneratePostBody()
		{
			var spider = new DefaultSpider("test", new Site
			{
				Arguments = new Dictionary<string, string>
				{
					{"TOKEN1", "TEST1"}
				}
			});
			TestDownloader downloader = new TestDownloader
			{
				BeforeDownloadHandlers = new IBeforeDownloadHandler[]
				{
					new GeneratePostBodyHandler
					{
						ArgumnetNames = new[] {"TOKEN1","TOKEN2"}
					}
				}
			};
			var request1 = new Request("http://a.com/", 0, new Dictionary<string, dynamic>
			{
				{"TOKEN2", "TEST2"}
			})
			{
				PostBody = "{0}{1}"
			};
			Page page = downloader.Download(request1, spider);
			Assert.Equal("TEST1TEST2", page.Request.PostBody);

			var request2 = new Request("http://a.com/", 0, new Dictionary<string, dynamic>())
			{
				PostBody = "{0}{1}"
			};
			page = downloader.Download(request2, spider);
			Assert.Equal("TEST1", page.Request.PostBody);

			request1 = new Request("http://a.com/", 0, new Dictionary<string, dynamic>
			{
				{"TOKEN2", "TEST2"}
			})
			{
				PostBody = "{0}{1}"
			};
			var spider2 = new DefaultSpider("test", new Site());
			page = downloader.Download(request1, spider2);
			Assert.Equal("TEST2", page.Request.PostBody);
		}

		[Fact]
		public void SubContentHandler()
		{
			var spider = new DefaultSpider("test", new Site());
			TestDownloader downloader1 = new TestDownloader()
			{
				DownloadCompleteHandlers = new IDownloadCompleteHandler[]
				{
					new SubContentHandler
					{
						Start = "a",
						End = "c"
					}
				}
			};
			var request1 = new Request("http://a.com/", 0, null);
			Page page = downloader1.Download(request1, spider);
			Assert.Equal("aabbc", page.Content);

			downloader1 = new TestDownloader
			{
				DownloadCompleteHandlers = new IDownloadCompleteHandler[]
				{
					new SubContentHandler
					{
						Start = "a",
						End = "c",
						EndOffset = 1
					}
				}
			};

			page = downloader1.Download(request1, spider);
			Assert.Equal("aabb", page.Content);

			downloader1 = new TestDownloader
			{
				DownloadCompleteHandlers = new IDownloadCompleteHandler[]
				{
					new SubContentHandler
					{
						Start = "a",
						End = "c",
						StartOffset = 1
					}
				}
			};

			page = downloader1.Download(request1, spider);
			Assert.Equal("abbc", page.Content);

			downloader1 = new TestDownloader
			{
				DownloadCompleteHandlers = new IDownloadCompleteHandler[]
				{
					new SubContentHandler
					{
						Start = "a",
						End = "c",
						StartOffset = 1,
						EndOffset = 1
					}
				}
			};

			page = downloader1.Download(request1, spider);
			Assert.Equal("abb", page.Content);

			downloader1 = new TestDownloader
			{
				DownloadCompleteHandlers = new IDownloadCompleteHandler[]
				{
					new SubContentHandler
					{
						Start = "a",
						End = "c",
						StartOffset = 10
					}
				}
			};

			var downloader2 = downloader1;
			var exception = Assert.Throws<SpiderException>(() =>
			{
				page = downloader2.Download(request1, spider);
			});
			Assert.Equal("Sub content failed. Please check your settings.", exception.Message);

			downloader1 = new TestDownloader
			{
				DownloadCompleteHandlers = new IDownloadCompleteHandler[]
				{
					new SubContentHandler
					{
						Start = "a",
						End = "c",
						EndOffset = 20
					}
				}
			};

			exception = Assert.Throws<SpiderException>(() =>
			{
				page = downloader1.Download(request1, spider);
			});
			Assert.Equal("Sub content failed. Please check your settings.", exception.Message);
		}

		[Fact]
		public void RetryWhenContainsIllegalStringHandler()
		{
			var spider = new DefaultSpider("test", new Site());
			TestDownloader downloader1 = new TestDownloader()
			{
				DownloadCompleteHandlers = new IDownloadCompleteHandler[]
				{
					new RetryWhenContainsIllegalStringHandler
					{
						ContainString = "网易"
					}
				}
			};
			var request1 = new Request("http://www.163.com/", 0, null);
			Page page = downloader1.Download(request1, spider);
			Assert.Equal(1, page.TargetRequests.Count);

			downloader1 = new TestDownloader
			{
				DownloadCompleteHandlers = new IDownloadCompleteHandler[]
				{
					new RetryWhenContainsIllegalStringHandler
					{
						ContainString = "网易倒闭啦"
					}
				}
			};

			page = downloader1.Download(request1, spider);
			Assert.Equal(0, page.TargetRequests.Count);
		}
	}
}
