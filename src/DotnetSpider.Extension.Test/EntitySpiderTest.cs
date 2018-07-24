using DotnetSpider.Core;
using DotnetSpider.Core.Monitor;
using DotnetSpider.Extension.Pipeline;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using Xunit;
using System.Linq;
using DotnetSpider.Extraction.Model.Attribute;
using DotnetSpider.Common;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model.Formatter;
using DotnetSpider.Extraction.Model;

namespace DotnetSpider.Extension.Test
{
	public class EntitySpiderTest : TestBase
	{
		public EntitySpiderTest()
		{
			Env.HubService = false;
		}

		[Fact(DisplayName = "MultiEntitySpider")]
		public void MultiEntity()
		{
			EntitySpider spider = new MultiEntitySpider();
			spider.Run();
			var pipeline = spider.Pipelines.ElementAt(0) as CollectionEntityPipeline;
			var neteast = pipeline.GetCollection("test.neteast").First() as MultiEntitySpider.NeteastEntity;
			var sohu = pipeline.GetCollection("test.sohu").First() as MultiEntitySpider.SohuEntity;
			Assert.Equal("搜狐", sohu.Title);
			Assert.Equal("网易", neteast.Title);
		}

#if Release
		[Fact]
		public void RedisKeepConnect()
		{
			var confiruation = new ConfigurationOptions()
			{
				ServiceName = "DotnetSpider",
				ConnectTimeout = 65530,
				KeepAlive = 8,
				ConnectRetry = 3,
				ResponseTimeout = 3000,
				AllowAdmin = true
			};

			confiruation.EndPoints.Add(new DnsEndPoint("127.0.0.1", 6379));

			var redis = ConnectionMultiplexer.Connect(confiruation);
			var db = redis.GetDatabase(1);

			var key = Guid.NewGuid().ToString("N");
			while (!db.LockTake(key, "0", TimeSpan.FromMinutes(10)))
			{
				Thread.Sleep(1000);
			}

			Thread.Sleep(240000);

			db.LockRelease(key, 0);
		}

#endif

		[Fact(DisplayName = "CleanSchedulerAfterCompleted")]
		public void CleanSchedulerAfterCompleted()
		{
			EntitySpider spider = new ClearSchedulerSpider();

			spider.Run();

			var confiruation = new ConfigurationOptions()
			{
				ServiceName = "DotnetSpider",
				ConnectTimeout = 65530,
				KeepAlive = 8,
				ConnectRetry = 3,
				ResponseTimeout = 3000,
				AllowAdmin = true
			};

			confiruation.EndPoints.Add(new DnsEndPoint("127.0.0.1", 6379));

			var redis = ConnectionMultiplexer.Connect(confiruation);
			var db = redis.GetDatabase(0);

			var md5 = Cryptography.ToShortMd5(spider.Identity);
			var itemKey = "item-" + md5;
			var setKey = "set-" + md5;
			var queueKey = "queue-" + md5;
			var errorCountKey = "error-record" + md5;
			var successCountKey = "success-record" + md5;

			//queue
			Assert.Equal(0, db.ListLength(queueKey));
			//set
			Assert.Equal(0, db.SetLength(setKey));
			//item
			Assert.Equal(0, db.HashLength(itemKey));
			//error-count
			Assert.False(db.StringGet(errorCountKey).HasValue);
			//success-count
			Assert.False(db.StringGet(successCountKey).HasValue);
		}

		[Fact(DisplayName = "GetPipelineFromAppConfig")]
		public void GetPipelineFromAppConfig()
		{
			var configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
			{
				ExeConfigFilename = "app.config"
			}, ConfigurationUserLevel.None);
			var pipeline1 = DbModelPipeline.GetPipelineFromAppConfig(configuration.ConnectionStrings.ConnectionStrings["DataConnection"]);
			Assert.True(pipeline1 is MySqlEntityPipeline);

			var pipeline2 = DbModelPipeline.GetPipelineFromAppConfig(configuration.ConnectionStrings.ConnectionStrings["SqlServerDataConnection"]);
			Assert.True(pipeline2 is SqlServerEntityPipeline);

			var pipeline3 = DbModelPipeline.GetPipelineFromAppConfig(configuration.ConnectionStrings.ConnectionStrings["MongoDbDataConnection"]);
			Assert.True(pipeline3 is MongoDbEntityPipeline);
		}

		[Fact(DisplayName = "EntitySpider")]
		public void EntitySpiderRunCorrect()
		{
			CasSpider spider = new CasSpider();
			spider.Run();
		}

		[TableInfo("test", "table")]
		private class TestEntity
		{
			[FieldSelector(Expression = ".")]
			public string Name { get; set; }
		}

		private class ClearSchedulerSpider : EntitySpider
		{
			public ClearSchedulerSpider() : base("ClearSchedulerTestSpider", new Site())
			{
			}

			protected override void OnInit(params string[] arguments)
			{
				Monitor = new LogMonitor();
				Identity = Guid.NewGuid().ToString("N");
				//Scheduler = new RedisScheduler("127.0.0.1:6379,serviceName=Scheduler.NET,keepAlive=8,allowAdmin=True,connectTimeout=10000,password=,abortConnect=True,connectRetry=20");
				AddStartUrl("https://baidu.com");
				AddPipeline(new ConsoleEntityPipeline());
				AddEntityType<TestEntity>();
			}
		}

		private class BaiduSearchSpider : EntitySpider
		{
			private readonly string _guid;

			public BaiduSearchSpider(string guid) : base("BaiduSearch")
			{
				_guid = guid;
			}

			protected override void OnInit(params string[] arguments)
			{
				var word = "可乐|雪碧";
				AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word),
					new Dictionary<string, dynamic> {
						{ "Keyword", word },
						{ "guid", _guid }
					});
				AddEntityType<BaiduSearchEntry>();
			}

			[TableInfo("test", "baidu_search")]
			[EntitySelector(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
			class BaiduSearchEntry
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
		}

		private class CasSpider : EntitySpider
		{
			public CasSpider() : base("casTest", new Site())
			{
			}

			protected override void OnInit(params string[] arguments)
			{
				Identity = Guid.NewGuid().ToString();
				EmptySleepTime = 5000;
				AddPipeline(new CollectionEntityPipeline());
				AddStartUrl("http://www.cas.cn/kx/kpwz/index.shtml");
				AddEntityType<ArticleSummary>();
			}

			[EntitySelector(Expression = "//div[@class='ztlb_ld_mainR_box01_list']/ul/li")]
			class ArticleSummary
			{
				[FieldSelector(Expression = ".//a/@title")]
				public string Title { get; set; }

				[ToNext(Extras = new[] { "Title", "Url" })]
				[FieldSelector(Expression = ".//a/@href")]
				public string Url { get; set; }
			}
		}

		private class MultiEntitySpider : EntitySpider
		{
			protected override void OnInit(params string[] arguments)
			{
				Site = new Site
				{
					Headers = new Dictionary<string, string>
					{
						{ "Upgrade-Insecure-Requests", "1"}
					}
				};
				EmptySleepTime = 6000;
				AddPipeline(new CollectionEntityPipeline());
				AddStartUrl("http://www.163.com");
				AddStartUrl("http://www.sohu.com");
				AddEntityType<NeteastEntity>();
				AddEntityType<SohuEntity>();
			}

			[TableInfo("test", "neteast")]
			[TargetRequestSelector(Patterns = new[] { "http://www.163.com" })]
			public class NeteastEntity
			{
				[FieldSelector(Expression = ".//title")]
				public string Title { get; set; }
			}

			[TableInfo("test", "sohu")]
			[TargetRequestSelector(Patterns = new[] { "http://www.sohu.com" })]
			public class SohuEntity
			{
				[FieldSelector(Expression = ".//title")]
				public string Title { get; set; }
			}
		}

		private static void CleanDb()
		{
			var confiruation = new ConfigurationOptions()
			{
				ServiceName = "DotnetSpider",
				ConnectTimeout = 65530,
				KeepAlive = 8,
				ConnectRetry = 3,
				ResponseTimeout = 3000,
				AllowAdmin = true
			};

			confiruation.EndPoints.Add(new DnsEndPoint("127.0.0.1", 6379));

			var redis = ConnectionMultiplexer.Connect(confiruation);
			var server = redis.GetServer(redis.GetEndPoints()[0]);
			server.FlushAllDatabases();
		}
	}
}
