using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Monitor;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Downloader;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;

namespace DotnetSpider.Extension.Test.Downloader
{
	public class WebDriverDownloaderTests
	{
		public WebDriverDownloaderTests()
		{
			Env.EnterpiseService = false;
		}

		[Fact]
		public void DestoryDownloader()
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return;
			}

			var chromedriverCount1 = Process.GetProcessesByName("chromedriver").Length;

			BaiduSearchSpider spider = new BaiduSearchSpider();
			spider.Run();

			var chromedriverCount2 = Process.GetProcessesByName("chromedriver").Length;

			Assert.Equal(chromedriverCount1, chromedriverCount2);
		}


		[Fact]
		public void ChromeHeadlessDownloader()
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return;
			}

			BaiduSearchHeadlessSpider spider = new BaiduSearchHeadlessSpider();
			spider.Run();
		}

		public class BaiduSearchHeadlessSpider : EntitySpider
		{
			public BaiduSearchHeadlessSpider() : base("BaiduSearchTest2")
			{
			}

			protected override void MyInit(params string[] arguments)
			{
				Monitor = new NLogMonitor();
				Identity = "hello";
				var word = "可乐|雪碧";
				AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });
				Downloader = new WebDriverDownloader(Browser.Chrome, new Option
				{
					Headless = true
				});
				EmptySleepTime = 6000;
				AddEntityType<BaiduSearchEntry>();
			}
		}

		public class BaiduSearchSpider : EntitySpider
		{
			public BaiduSearchSpider() : base("BaiduSearchTest")
			{
			}

			protected override void MyInit(params string[] arguments)
			{
				Monitor = new NLogMonitor();
				Identity = "hello";
				var word = "可乐|雪碧";
				AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });
				Downloader = new WebDriverDownloader(Browser.Chrome);
				AddEntityType<BaiduSearchEntry>();
			}
		}

		[EntityTable("baidu", "baidu_search")]
		[EntitySelector(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
		public class BaiduSearchEntry : SpiderEntity
		{
			[PropertyDefine(Expression = "Keyword", Type = SelectorType.Enviroment)]
			public string Keyword { get; set; }

			[PropertyDefine(Expression = ".//h3[@class='c-title']/a")]
			[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
			[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
			public string Title { get; set; }

			[PropertyDefine(Expression = ".//h3[@class='c-title']/a/@href")]
			public string Url { get; set; }

			[PropertyDefine(Expression = ".//div/p[@class='c-author']/text()")]
			[ReplaceFormatter(NewValue = "-", OldValue = "&nbsp;")]
			public string Website { get; set; }


			[PropertyDefine(Expression = ".//div/span/a[@class='c-cache']/@href")]
			public string Snapshot { get; set; }


			[PropertyDefine(Expression = ".//div[@class='c-summary c-row ']", Option = PropertyDefine.Options.PlainText)]
			[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
			[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
			[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
			public string Details { get; set; }

			[PropertyDefine(Expression = ".", Option = PropertyDefine.Options.PlainText)]
			[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
			[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
			[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
			public string PlainText { get; set; }

			[PropertyDefine(Expression = "today", Type = SelectorType.Enviroment)]
			public DateTime RunId { get; set; }
		}
	}
}