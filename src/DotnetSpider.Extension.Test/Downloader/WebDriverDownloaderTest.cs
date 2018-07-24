using DotnetSpider.Core;
using DotnetSpider.Core.Monitor;
using DotnetSpider.Extension.Downloader;
using DotnetSpider.Extension.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extraction.Model.Attribute;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model.Formatter;
using DotnetSpider.Common;
using DotnetSpider.Extraction.Model;
#if NETSTANDARD
using System.Runtime.InteropServices;
#endif

namespace DotnetSpider.Extension.Test.Downloader
{
	public class WebDriverDownloaderTest
	{
		public WebDriverDownloaderTest()
		{
			Env.HubService = false;
		}

		[Fact(DisplayName = "WebDriverDownloader_DestoryDownloader")]
		public void DestoryDownloader()
		{
#if NETSTANDARD
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return;
			}
#endif

			var chromedriverCount1 = Process.GetProcessesByName("chromedriver").Length;

			WebDriverDownloaderSpider spider = new WebDriverDownloaderSpider();
			spider.Run();

			var chromedriverCount2 = Process.GetProcessesByName("chromedriver").Length;

			Assert.Equal(chromedriverCount1, chromedriverCount2);
		}


		[Fact(DisplayName = "WebDriverDownloader_ChromeHeadlessDownloader")]
		public void ChromeHeadlessDownloader()
		{
#if NETSTANDARD
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return;
			}
#endif
			HeadlessSpider spider = new HeadlessSpider();
			spider.Run();
		}

		private class HeadlessSpider : EntitySpider
		{
			public HeadlessSpider() : base("HeadlessSpider")
			{
			}

			protected override void OnInit(params string[] arguments)
			{
				Monitor = new LogMonitor();
				Identity = "HeadlessSpider";
				var word = "可乐|雪碧";
				AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });
				Downloader = new WebDriverDownloader(Browser.Chrome, new Option
				{
					Headless = true
				});
				EmptySleepTime = 6000;
				AddPipeline(new ConsoleEntityPipeline());
				AddEntityType<BaiduSearchEntry>();
			}
		}

		private class WebDriverDownloaderSpider : EntitySpider
		{
			public WebDriverDownloaderSpider() : base("WebDriverDownloader")
			{
			}

			protected override void OnInit(params string[] arguments)
			{
				var word = "可乐|雪碧";
				AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });
				Downloader = new WebDriverDownloader(Browser.Chrome);
				AddPipeline(new ConsoleEntityPipeline());
				AddEntityType<BaiduSearchEntry>();
			}
		}

		[TableInfo("baidu", "baidu_search")]
		[EntitySelector(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
		private class BaiduSearchEntry
		{
			[FieldSelector(Expression = "Keyword", Type = SelectorType.Enviroment)]
			public string Keyword { get; set; }

			[FieldSelector(Expression = ".//h3[@class='c-title']/a")]
			[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
			[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
			public string Title { get; set; }

			[FieldSelector(Expression = ".//h3[@class='c-title']/a/@href")]
			public string Url { get; set; }

			[FieldSelector(Expression = ".//div/p[@class='c-author']/text()")]
			[ReplaceFormatter(NewValue = "-", OldValue = "&nbsp;")]
			public string Website { get; set; }


			[FieldSelector(Expression = ".//div/span/a[@class='c-cache']/@href")]
			public string Snapshot { get; set; }


			[FieldSelector(Expression = ".//div[@class='c-summary c-row ']", Option = FieldOptions.InnerText)]
			[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
			[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
			[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
			public string Details { get; set; }

			[FieldSelector(Expression = ".", Option = FieldOptions.InnerText)]
			[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
			[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
			[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
			public string PlainText { get; set; }

			[FieldSelector(Expression = "today", Type = SelectorType.Enviroment)]
			public DateTime RunId { get; set; }
		}
	}
}