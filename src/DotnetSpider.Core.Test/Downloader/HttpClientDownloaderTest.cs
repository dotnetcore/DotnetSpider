using System.Collections.Generic;
using System.Text;
using DotnetSpider.Core.Downloader;
using Xunit;
using DotnetSpider.Core.Scheduler;
using static DotnetSpider.Core.Test.SpiderTest;
using DotnetSpider.Core.Pipeline;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Test.Downloader
{
	public class HttpClientDownloaderTest
	{
		public HttpClientDownloaderTest()
		{
#if !NET45
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif

		}

		/// <summary>
		/// 手动执行此测试脚本，运行结束后用netstat -ano 查看端口占用情况。只会占用一个就对了
		/// </summary>
		[Fact(Skip = "Need person double check")]
		public void Ports()
		{
			HttpClientDownloader downloader = new HttpClientDownloader();
			DefaultSpider spider = new DefaultSpider("abcd", new Site { });

			for (int i = 0; i < 100; i++)
			{
				var a = downloader.Download(new Request("http://www.163.com", null), spider).Result;
			}
		}

		[Fact(DisplayName = "DetectDownloadContent")]
		public void DetectDownloadContent()
		{
			HttpClientDownloader downloader = new HttpClientDownloader();
			DefaultSpider spider = new DefaultSpider("abcd", new Site { });

			var a = downloader.Download(new Request("http://www.163.com", null), spider);
			Assert.Equal(Core.Infrastructure.ContentType.Html, spider.Site.ContentType);

			HttpClientDownloader2 downloader2 = new HttpClientDownloader2();
			DefaultSpider spider2 = new DefaultSpider("abcd", new Site { });
			a = downloader2.Download(new Request("http://www.163.com", null), spider2);
			Assert.Equal(Core.Infrastructure.ContentType.Json, spider2.Site.ContentType);
		}

		[Fact(DisplayName = "SetContentType")]
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
			var a = downloader.Download(new Request("http://163.com", null), new DefaultSpider("test", site1));

			a = downloader.Download(new Request("http://163.com", null), new DefaultSpider("test", site2));
		}

		[Fact(DisplayName = "_404Url")]
		public void _404Url()
		{
			if (!Env.IsWindows)
			{
				return;
			}
			var spider = Spider.Create(new Site { EncodingName = "UTF-8", SleepTime = 1000 },
					"abcd",
					new QueueDuplicateRemovedScheduler(),
					new TestPageProcessor());
			spider.AddPipeline(new ConsolePipeline());
			spider.SkipTargetUrlsWhenResultIsEmpty = false;
			spider.EmptySleepTime = 6000;
			spider.AddStartUrl("http://www.mlr.gov.cn/xwdt/jrxw/201707/t20170710_15242382.htm");
			spider.Run();
			Assert.Equal(5, spider.RetriedTimes.Value);
		}

		[Fact(DisplayName = "_301Url")]
		public void _301Url()
		{
			if (!Env.IsWindows)
			{
				return;
			}
			var spider = Spider.Create(new Site { EncodingName = "UTF-8", SleepTime = 1000 },
					"abcd",
					new QueueDuplicateRemovedScheduler(),
					new TestPageProcessor());
			spider.AddPipeline(new ConsolePipeline());
			spider.SkipTargetUrlsWhenResultIsEmpty = true;
			spider.Downloader = new HttpClientDownloader();
			spider.EmptySleepTime = 6000;
			spider.AddStartUrl("https://tieba.baidu.com/f?kw=%E7%AE%80%E9%98%B3&ie=utf-8&pn=50");
			spider.Run();
			Assert.Equal(0, spider.RetriedTimes.Value);
		}

		class HttpClientDownloader2 : HttpClientDownloader
		{
			protected override Task<Page> DowloadContent(Request request, ISpider spider)
			{
				var page = new Page(request) { Content = "{'a':'b'}" };
				return Task.FromResult(page);
			}
		}

		[Fact(DisplayName = "GetTargetUrlWhenRedirect")]
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
			var page = downloader.Download(new Request("http://item.jd.com/1231222221111123.html", null), new DefaultSpider("test", site)).Result;
			Assert.DoesNotContain("1231222221111123", page.TargetUrl);
			Assert.True(page.TargetUrl.Contains("www.jd.com/") || page.TargetUrl.Contains("global.jd.com"));
		}

		[Fact(DisplayName = "SetTimeout")]
		public void SetTimeout()
		{
			HttpClientDownloader downloader = new HttpClientDownloader();
			var entry = HttpClientDownloader.HttpClientPool.GetHttpClient("a");
			downloader.PrepareHttpClient(entry);
			Assert.Equal(8, entry.Client.Timeout.TotalSeconds);
		}
	}
}
