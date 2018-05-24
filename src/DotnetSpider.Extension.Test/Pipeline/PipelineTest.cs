﻿using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.Pipeline;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DotnetSpider.Extension.Test.Pipeline
{
	public class PipelineTest
	{
		[Fact]
		public void MixProcessorAndMissEntityPipeline()
		{
			var exceptoin = Assert.Throws<SpiderException>(() =>
			{
				var id = Guid.NewGuid().ToString("N");
				BaiduSearchSpider spider = new BaiduSearchSpider();
				spider.Identity = id;
				spider.Run();
			});
			Assert.Equal("You may miss a entity pipeline.", exceptoin.Message);
		}

		[Fact]
		public void MixProcessor()
		{
			var id = Guid.NewGuid().ToString("N");
			BaiduSearchSpider spider = new BaiduSearchSpider();
			spider.AddPipeline(new MySqlEntityPipeline());
			spider.Identity = id;
			spider.Run();

			using (var conn = new MySqlConnection(Env.DataConnectionString))
			{
				var count = conn.QueryFirst<int>("SELECT COUNT(*) FROM baidu.baidu_search_mixprocessor");
				Assert.Equal(20, count);
				conn.Execute("DROP TABLE baidu.baidu_search_mixprocessor");
			}
		}


		public class BaiduSearchSpider : EntitySpider
		{
			public BaiduSearchSpider() : base("BaiduSearchSpider")
			{
			}

			protected override void MyInit(params string[] arguments)
			{
				//var word = "可乐|雪碧";
				//AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });

                AddStartUrl("http://news.baidu.com/ns?word=可乐|雪碧&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", new Dictionary<string, dynamic> { { "Keyword", "可乐|雪碧" } });


                EmptySleepTime = 1000;
				AddEntityType<BaiduSearchEntry>();
				AddPageProcessor(new MyProcessor());
				AddPipeline(new MyPipeline());
			}

			class MyPipeline : BasePipeline
			{
				public override void Process(params ResultItems[] resultItems)
				{
				}
			}

			class MyProcessor : BasePageProcessor
			{
				protected override void Handle(Page page)
				{
				}
			}

			[EntityTable("baidu", "baidu_search_mixprocessor")]
			[EntitySelector(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
			class BaiduSearchEntry : SpiderEntity
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
}
