using DotnetSpider.Core.Downloader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DotnetSpider.Core.Test.SpiderTest;

namespace DotnetSpider.Core.Test
{
	[TestClass]
	public class CookieInjectorTest
	{
		[TestMethod]
		public void InjectCookiesBeforeSpiderRun()
		{
			var path = "www.baidu.com.cookies";
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			File.WriteAllText(path, "a=b&c=d");
			FileCookieInject inject = new FileCookieInject();
			Site site = new Site();
			site.AddStartUrl("http://www.baidu.com");
			DefaultSpider spider = new DefaultSpider("a", site);
			inject.Inject(spider, false);
			Assert.AreEqual("a=b&c=d", site.Cookies.ToString());
		}

		[TestMethod]
		public void InjectCookiesWhenSpiderRun()
		{
			var path = "www.baidu.com.cookies";
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			File.WriteAllText(path, "a=b&c=d");

			Spider spider = Spider.Create(new Site { EncodingName = "UTF-8", SleepTime = 1000 }, new TestPageProcessor()).AddPipeline(new TestPipeline()).SetThreadNum(1);
			spider.SetDownloader(new HttpClientDownloader()
			{
				DownloadCompleteHandlers = new IDownloadCompleteHandler[]
				{
					new TimerUpdateCookieHandler(5,new FileCookieInject())
				}
			});
			for (int i = 0; i < 10000; i++)
			{
				spider.AddStartUrl("http://www.baidu.com/" + i);
			}
			spider.Run();
		}
	}
}
