using System.Collections.Generic;
using DotnetSpider.Core.Downloader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Core.Scheduler;
using static DotnetSpider.Core.Test.SpiderTest;
using DotnetSpider.Core.Pipeline;

namespace DotnetSpider.Core.Test.Downloader
{
	[TestClass]
	public class HttpClientDownloaderTest
	{
		//[TestMethod]
		//public void Timeout()
		//{
		//	HttpClientDownloader downloader = new HttpClientDownloader();
		//	DefaultSpider spider = new DefaultSpider("abcd", new Site { Timeout = 5000 });
		//	downloader.Download(new Request("http://www.163.com", null), spider);
		//	try
		//	{
		//		downloader.Download(new Request("http://localhost/abcasdfasdfasdfas", null), spider);
		//		throw new Exception("Test Failed");
		//	}
		//	catch (SpiderException e)
		//	{
		//		Assert.IsNotNull(e);
		//	}
		//	Stopwatch watch = new Stopwatch();
		//	watch.Start();
		//	try
		//	{
		//		downloader.Download(new Request("http://google.com/", null), spider);
		//	}
		//	catch (SpiderException e)
		//	{
		//		Assert.IsNotNull(e);
		//	}
		//	watch.Stop();
		//	Assert.IsTrue(watch.ElapsedMilliseconds > 5000);
		//	Assert.IsTrue(watch.ElapsedMilliseconds < 6000);
		//}

		/// <summary>
		/// 手动执行此测试脚本，运行结束后用netstat -ano 查看端口占用情况。只会占用一个就对了。如果
		/// </summary>
		[Ignore]
		[TestMethod]
		public void Ports()
		{
			HttpClientDownloader downloader = new HttpClientDownloader();
			DefaultSpider spider = new DefaultSpider("abcd", new Site { Timeout = 5000 });

			for (int i = 0; i < 100; i++)
			{
				downloader.Download(new Request("http://www.163.com", null), spider);
			}
		}

		[TestMethod]
		public void DetectDownloadContent()
		{
			HttpClientDownloader downloader = new HttpClientDownloader();
			DefaultSpider spider = new DefaultSpider("abcd", new Site { Timeout = 5000 });

			downloader.Download(new Request("http://www.163.com", null), spider);
			Assert.AreEqual(ContentType.Html, spider.Site.ContentType);

			HttpClientDownloader2 downloader2 = new HttpClientDownloader2();
			DefaultSpider spider2 = new DefaultSpider("abcd", new Site { Timeout = 5000 });
			downloader2.Download(new Request("http://www.163.com", null), spider2);
			Assert.AreEqual(ContentType.Json, spider2.Site.ContentType);
		}

		[TestMethod]
		public void SetContentType()
		{
			Site site1 = new Site
			{
				Headers = new Dictionary<string, string>()
				{
					{"Content-Type","abcd" }
				}
			};
			Site site2 = new Site
			{
				Headers = new Dictionary<string, string>()
				{
					{"ContentType","abcd" }
				}
			};
			HttpClientDownloader downloader = new HttpClientDownloader();
			downloader.Download(new Request("http://163.com", null), new DefaultSpider("test", site1));

			downloader.Download(new Request("http://163.com", null), new DefaultSpider("test", site2));
		}

		[TestMethod]
		public void Test404Url()
		{
			var spider = Spider.Create(new Site { EncodingName = "UTF-8", SleepTime = 1000 },
					"abcd",
					new QueueDuplicateRemovedScheduler(),
					new TestPageProcessor());
			spider.AddPipeline(new ConsolePipeline());
			spider.AddStartUrl("http://www.mlr.gov.cn/xwdt/jrxw/201707/t20170710_15242382.htm");
			spider.Run();
			Assert.AreEqual(4, spider.RetriedTimes.Value);
		}

		class HttpClientDownloader2 : HttpClientDownloader
		{
			protected override Page DowloadContent(Request request, ISpider spider)
			{
				return new Page(request) { Content = "{'a':'b'}" };
			}
		}
	}
}
