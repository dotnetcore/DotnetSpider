using System.Collections.Generic;
using DotnetSpider.Core.Downloader;
using Xunit;
using DotnetSpider.Core.Scheduler;
using static DotnetSpider.Core.Test.SpiderTest;
using DotnetSpider.Core.Pipeline;

namespace DotnetSpider.Core.Test.Downloader
{

	public class HttpClientDownloaderTest
	{
		//[Fact]
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
		//	Assert.True(watch.ElapsedMilliseconds > 5000);
		//	Assert.True(watch.ElapsedMilliseconds < 6000);
		//}

		/// <summary>
		/// 手动执行此测试脚本，运行结束后用netstat -ano 查看端口占用情况。只会占用一个就对了。如果
		/// </summary>
		[Fact(Skip = "Need person double check")]
		public void Ports()
		{
			HttpClientDownloader downloader = new HttpClientDownloader();
			DefaultSpider spider = new DefaultSpider("abcd", new Site { Timeout = 5000 });

			for (int i = 0; i < 100; i++)
			{
				downloader.Download(new Request("http://www.163.com", null), spider);
			}
		}

		[Fact]
		public void DetectDownloadContent()
		{
			HttpClientDownloader downloader = new HttpClientDownloader();
			DefaultSpider spider = new DefaultSpider("abcd", new Site { Timeout = 5000 });

			downloader.Download(new Request("http://www.163.com", null), spider);
			Assert.Equal(ContentType.Html, spider.Site.ContentType);

			HttpClientDownloader2 downloader2 = new HttpClientDownloader2();
			DefaultSpider spider2 = new DefaultSpider("abcd", new Site { Timeout = 5000 });
			downloader2.Download(new Request("http://www.163.com", null), spider2);
			Assert.Equal(ContentType.Json, spider2.Site.ContentType);
		}

		[Fact]
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
			var downloader = new HttpClientDownloader();
			downloader.Download(new Request("http://163.com", null), new DefaultSpider("test", site1));

			downloader.Download(new Request("http://163.com", null), new DefaultSpider("test", site2));
		}

		[Fact]
		public void _404Url()
		{
			var spider = Spider.Create(new Site { EncodingName = "UTF-8", SleepTime = 1000 },
					"abcd",
					new QueueDuplicateRemovedScheduler(),
					new TestPageProcessor());
			spider.AddPipeline(new ConsolePipeline());
			spider.SkipWhenResultIsEmpty = false;
			spider.AddStartUrl("http://www.mlr.gov.cn/xwdt/jrxw/201707/t20170710_15242382.htm");
			spider.Run();
			Assert.Equal(5, spider.RetriedTimes.Value);
		}

		class HttpClientDownloader2 : HttpClientDownloader
		{
			protected override Page DowloadContent(Request request, ISpider spider)
			{
				return new Page(request) { Content = "{'a':'b'}" };
			}
		}

		[Fact]
		public void GetTargetUrlWhenRedirect()
		{
			Site site = new Site
			{
				Headers = new Dictionary<string, string>
				{
					{ "User-Agent", "Chrome" }
				}
			};
			var downloader = new HttpClientDownloader();
			var page = downloader.Download(new Request("http://item.jd.com/1231222221111123.html", null), new DefaultSpider("test", site));
			Assert.EndsWith("www.jd.com/?d", page.TargetUrl);
		}
	}
}
