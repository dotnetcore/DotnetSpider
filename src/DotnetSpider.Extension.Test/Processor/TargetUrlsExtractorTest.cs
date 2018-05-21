using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure.Database;
using DotnetSpider.Core.Monitor;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using System;
using System.Collections.Generic;
using Xunit;

namespace DotnetSpider.Extension.Test.Processor
{
	public class TargetUrlsExtractorTest
	{
		[Fact(DisplayName = "AutoIncrementTargetUrlsExtractor_Test")]
		public void AutoIncrementTargetUrlsExtractor_Test()
		{
			var id = Guid.NewGuid().ToString("N");
			BaiduSearchSpider spider = new BaiduSearchSpider(id);
			spider.Run();
			using (var conn = Env.DataConnectionStringSettings.CreateDbConnection())
			{
				var count = conn.QueryFirst<int>($"SELECT COUNT(*) FROM test.baidu_search WHERE guid='{id}'");
				Assert.Equal(60, count);
			}
		}

		private class TestTargetUrlsExtractorTermination : ITargetUrlsExtractorTermination
		{
			public bool IsTermination(Page page)
			{
				return page.Url.Contains("&pn=2");
			}
		}

		private class BaiduSearchSpider : EntitySpider
		{
			private readonly string _guid;

			public BaiduSearchSpider(string guid) : base("BaiduSearch")
			{
				_guid = guid;
			}

			protected override void MyInit(params string[] arguments)
			{
				Monitor = new LogMonitor();
				var word = "可乐|雪碧";
				Identity = Guid.NewGuid().ToString();
				AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word),
					new Dictionary<string, dynamic> {
						{ "Keyword", word },
						{ "guid", _guid }
					});
				AddEntityType<BaiduSearchEntry>(new AutoIncrementTargetUrlsExtractor("&pn=0", 1, new TestTargetUrlsExtractorTermination()));
			}

			[EntityTable("test", "baidu_search")]
			[EntitySelector(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
			class BaiduSearchEntry : SpiderEntity
			{
				[PropertyDefine(Expression = "Keyword", Type = SelectorType.Enviroment, Length = 100)]
				public string Keyword { get; set; }

				[PropertyDefine(Expression = "guid", Type = SelectorType.Enviroment, Length = 100)]
				public string Guid { get; set; }

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


				[PropertyDefine(Expression = ".//div[@class='c-summary c-row ']", Option = PropertyDefineOptions.InnerText)]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
				public string Details { get; set; }

				[PropertyDefine(Expression = ".", Option = PropertyDefineOptions.InnerText)]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
				public string PlainText { get; set; }
			}
		}
	}
}
