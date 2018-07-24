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
using DotnetSpider.Common;
using DotnetSpider.Core.Processor.TargetRequestExtractors;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Downloader;

namespace DotnetSpider.Extension.Test.Processor
{
	public class TargetRequestExtractorTest
	{
		[TargetRequestSelector()]
		public class Entity15
		{
			[FieldSelector(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}

		[TargetRequestSelector(XPaths = new[] { "" }, Patterns = new[] { "" })]
		public class Entity23
		{
			[FieldSelector(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}

		[TargetRequestSelector(XPaths = new string[] { null }, Patterns = new string[] { null })]
		public class Entity24
		{
			[FieldSelector(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}


		[Fact(DisplayName = "TargetRequestSelector_Null")]
		public void TargetRequestSelector_Null()
		{
			try
			{
				var processor = new EntityProcessor<Entity15>();
			}
			catch (Exception e)
			{
				Assert.Equal("Region xpath and patterns should not be null both", e.Message);
				return;
			}
			throw new Exception("Failed.");
		}

		[Fact(DisplayName = "TargetRequestSelector_EmptyRegion_EmptyPattern")]
		public void TargetRequestSelector_EmptyRegion_EmptyPattern()
		{
			try
			{
				var processor = new EntityProcessor<Entity23>();
			}
			catch (ArgumentException e)
			{
				Assert.NotNull(e);
				return;
			}
			throw new Exception("Failed.");
		}

		[Fact(DisplayName = "TargetRequestSelector_NullRegion_NullPattern")]
		public void TargetRequestSelector_NullRegion_NullPattern()
		{
			try
			{
				var processor = new EntityProcessor<Entity24>();
			}
			catch (ArgumentException e)
			{
				Assert.NotNull(e);
				return;
			}
			throw new Exception("Failed.");
		}

		[Fact(DisplayName = "AutoIncrementTargetUrlsExtractor_Test")]
		public void AutoIncrementTargetRequestExtractor_Test()
		{
			var id = Guid.NewGuid().ToString("N");
			AutoIncrementTargetRequestExtractorSpider spider = new AutoIncrementTargetRequestExtractorSpider(id);
			//spider.Downloader = new HttpClientDownloader { UseFiddlerProxy = true };
			spider.Run();
			var pipeline = spider.Pipelines.First() as CollectionEntityPipeline;
			var entities = pipeline.GetCollection("test.baidu_search");
			Assert.Equal(60, entities.Count());
		}

		private class TestTargetRequestExtractorTermination : ITargetRequestExtractorTermination
		{
			public bool IsTerminated(Response response)
			{
				return response.Request.Url.Contains("&pn=2");
			}
		}

		private class AutoIncrementTargetRequestExtractorSpider : EntitySpider
		{
			[TableInfo("test", "baidu_search")]
			[EntitySelector(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
			private class BaiduSearchEntry
			{
				[FieldSelector(Expression = "Keyword", Type = SelectorType.Enviroment, Length = 100)]
				public string Keyword { get; set; }

				[FieldSelector(Expression = "guid", Type = SelectorType.Enviroment, Length = 100)]
				public string Guid { get; set; }

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
			}
			private readonly string _guid;

			public AutoIncrementTargetRequestExtractorSpider(string guid) : base("BaiduSearch")
			{
				_guid = guid;
			}

			protected override void OnInit(params string[] arguments)
			{
				Site = new Site
				{
					Headers = new Dictionary<string, string>
					{
						{ "Upgrade-Insecure-Requests", "1" }
					},
					Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8"
				};
				Monitor = new LogMonitor();
				var word = "可乐|雪碧";
				Identity = Guid.NewGuid().ToString();
				AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word),
					new Dictionary<string, dynamic> {
						{ "Keyword", word },
						{ "guid", _guid }
					});
				AddEntityType<BaiduSearchEntry>(new AutoIncrementTargetRequestExtractor("&pn=0", 1, new TestTargetRequestExtractorTermination()));
				AddPipeline(new CollectionEntityPipeline());
			}
		}


	}
}
