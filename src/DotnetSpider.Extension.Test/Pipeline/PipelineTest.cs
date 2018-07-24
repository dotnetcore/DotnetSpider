using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Core.Monitor;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Extension.Pipeline;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using Xunit;
using DotnetSpider.Extraction.Model.Attribute;
using DotnetSpider.Extraction.Model.Formatter;
using DotnetSpider.Extraction;
using DotnetSpider.Common;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Downloader;

namespace DotnetSpider.Extension.Test.Pipeline
{
	public class PipelineTest : TestBase
	{
		class BaiduSearchSpider : EntitySpider
		{
			public BaiduSearchSpider() : base("BaiduSearchSpider")
			{
			}

			protected override void OnInit(params string[] arguments)
			{
				Monitor = new LogMonitor();
				AddStartUrl("http://news.baidu.com/ns?word=可乐|雪碧&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", new Dictionary<string, dynamic> { { "Keyword", "可乐|雪碧" } });
				EmptySleepTime = 1000;
				AddEntityType<BaiduSearchEntry>();
				AddPageProcessors(new MyProcessor());
				AddPipeline(new MyPipeline());
			}

			class MyProcessor : BasePageProcessor
			{
				protected override void Handle(Page page)
				{
				}
			}

			class MyPipeline : BasePipeline
			{
				public override void Process(IList<ResultItems> resultItems, ILogger logger, dynamic sender = null)
				{

				}
			}

			[TableInfo("baidu", "baidu_search_mixprocessor")]
			[EntitySelector(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
			class BaiduSearchEntry
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
				public DateTime run_id { get; set; }
			}
		}

		[Fact(Skip = "NEXT", DisplayName = "MixProcessorAndMissEntityPipeline")]
		public void MixProcessorAndMissEntityPipeline()
		{
			var exceptoin = Assert.Throws<SpiderException>(() =>
			{
				var id = Guid.NewGuid().ToString("N");
				BaiduSearchSpider spider = new BaiduSearchSpider();
				spider.Identity = id;
				spider.Run();
			});
			Assert.Equal("You may miss a entity pipeline", exceptoin.Message);
		}

		[Fact(DisplayName = "MixProcessor")]
		public void MixProcessor()
		{
			using (var conn = new MySqlConnection(DefaultConnectionString))
			{
				conn.Execute("DROP TABLE IF EXISTS baidu.baidu_search_mixprocessor");
			}
			var id = Guid.NewGuid().ToString("N");
			BaiduSearchSpider spider = new BaiduSearchSpider();
			spider.AddPipeline(new MySqlEntityPipeline(DefaultConnectionString));
			spider.Identity = id;
			spider.Run();

			using (var conn = new MySqlConnection(DefaultConnectionString))
			{
				var count = conn.QueryFirst<int>("SELECT COUNT(*) FROM baidu.baidu_search_mixprocessor");
				Assert.Equal(20, count);
				conn.Execute("DROP TABLE IF EXISTS baidu.baidu_search_mixprocessor");
			}
		}
	}
}
