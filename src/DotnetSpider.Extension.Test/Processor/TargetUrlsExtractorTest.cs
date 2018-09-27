using DotnetSpider.Core.Monitor;
using DotnetSpider.Core.Processor;
using DotnetSpider.Extension.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DotnetSpider.Extraction.Model.Formatter;
using DotnetSpider.Extraction.Model.Attribute;
using DotnetSpider.Extraction;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Downloader;
using DotnetSpider.Extension.Model;
using DotnetSpider.Core;
using DotnetSpider.Core.Processor.RequestExtractor;

namespace DotnetSpider.Extension.Test.Processor
{
	public class TargetRequestExtractorTest
	{
		[Target()]
		public class Entity15 : IBaseEntity
		{
			[Field(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}

		[Target(XPaths = new[] { "" }, Patterns = new[] { "" })]
		public class Entity23 : IBaseEntity
		{
			[Field(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}

		[Target(XPaths = new string[] { null }, Patterns = new string[] { null })]
		public class Entity24 : IBaseEntity
		{
			[Field(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}

		[Fact(DisplayName = "AutoIncrementTargetUrlsExtractor_Test")]
		public void AutoIncrementTargetRequestExtractor_Test()
		{
			var id = Guid.NewGuid().ToString("N");
			AutoIncrementTargetRequestExtractorSpider spider = new AutoIncrementTargetRequestExtractorSpider(id);
			spider.Run();
			var pipeline = spider.Pipelines.First() as CollectionEntityPipeline;
			var entities = pipeline.GetCollection("DotnetSpider.Extension.Test.Processor.TargetRequestExtractorTest+AutoIncrementTargetRequestExtractorSpider+BaiduSearchEntry");
			Assert.Equal(60, entities.Count());
		}

		private class TestLastPageChecker : ILastPageChecker
		{
			public bool IsLastPage(Page page)
			{
				return page.Request.Url.Contains("&pn=2");
			}
		}

		private class AutoIncrementTargetRequestExtractorSpider : EntitySpider
		{
			[Schema("test", "baidu_search")]
			[Entity(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
			private class BaiduSearchEntry : IBaseEntity
			{
				[Column]
				[Field(Expression = "Keyword", Type = SelectorType.Enviroment)]
				public string Keyword { get; set; }

				[Column]
				[Field(Expression = "guid", Type = SelectorType.Enviroment)]
				public string Guid { get; set; }

				[Column]
				[Field(Expression = ".//h3[@class='c-title']/a")]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				public string Title { get; set; }

				[Column]
				[Field(Expression = ".//h3[@class='c-title']/a/@href")]
				public string Url { get; set; }

				[Column]
				[Field(Expression = ".//div/p[@class='c-author']/text()")]
				[ReplaceFormatter(NewValue = "-", OldValue = "&nbsp;")]
				public string Website { get; set; }

				[Column]
				[Field(Expression = ".//div/span/a[@class='c-cache']/@href")]
				public string Snapshot { get; set; }

				[Column]
				[Field(Expression = ".//div[@class='c-summary c-row ']", Option = FieldOptions.InnerText)]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
				public string Details { get; set; }

				[Column(Length = 0)]
				[Field(Expression = ".", Option = FieldOptions.InnerText)]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
				public string PlainText { get; set; }
			}
			private readonly string _guid;

			public AutoIncrementTargetRequestExtractorSpider(string guid) : base("BaiduSearch")
			{
				_guid = guid;
			}

			protected override void OnInit(params string[] arguments)
			{
				Monitor = new LogMonitor();
				var word = "可乐|雪碧";
				Identity = Guid.NewGuid().ToString();
				AddRequest(
					new Request(
						string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word),
						new Dictionary<string, dynamic> {
							{ "Keyword", word },
							{ "guid", _guid }
						})
					{
						Headers = new Dictionary<string, object> { { "Upgrade-Insecure-Requests", "1" } },
						Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8"
					});

				AddEntityType<BaiduSearchEntry>()
					.SetRequestExtractor(new AutoIncrementRequestExtractor("&pn=0", 1))
					.SetLastPageChecker(new TestLastPageChecker());
				AddPipeline(new CollectionEntityPipeline());
			}
		}
	}
}
