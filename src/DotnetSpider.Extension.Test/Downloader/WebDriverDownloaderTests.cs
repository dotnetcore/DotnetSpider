using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Downloader.WebDriver;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.ORM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Test.Downloader
{
	[TestClass]
	public class WebDriverDownloaderTests
	{
		[TestMethod]
		public void DestoryDownloader()
		{
			BaiduSearchSpider spider = new BaiduSearchSpider();
			spider.Run();
		}

		public class BaiduSearchSpider : EntitySpider
		{
			public BaiduSearchSpider() : base("BaiduSearchTest")
			{
			}

			protected override void MyInit(params string[] arguments)
			{
				Identity = "hello";
				var word = "可乐|雪碧";
				AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });
				Downloader = new WebDriverDownloader(Core.Infrastructure.Browser.Chrome);
				AddEntityType(typeof(BaiduSearchEntry));
			}
		}

		[Table("baidu", "baidu_search")]
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
			public DateTime run_id { get; set; }
		}
	}
}