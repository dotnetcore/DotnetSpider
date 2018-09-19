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
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model.Formatter;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extension.Model;
using DotnetSpider.Core.Infrastructure;

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
			var neteast = pipeline.GetCollection("DotnetSpider.Extension.Test.EntitySpiderTest+MultiEntitySpider+NeteastEntity").First() as MultiEntitySpider.NeteastEntity;
			var sohu = pipeline.GetCollection("DotnetSpider.Extension.Test.EntitySpiderTest+MultiEntitySpider+SohuEntity").First() as MultiEntitySpider.SohuEntity;
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
			var pipeline1 = DbEntityPipeline.GetPipelineFromAppConfig(configuration.ConnectionStrings.ConnectionStrings["DataConnection"]);
			Assert.True(pipeline1 is MySqlEntityPipeline);

			var pipeline2 = DbEntityPipeline.GetPipelineFromAppConfig(configuration.ConnectionStrings.ConnectionStrings["SqlServerDataConnection"]);
			Assert.True(pipeline2 is SqlServerEntityPipeline);

			var pipeline3 = DbEntityPipeline.GetPipelineFromAppConfig(configuration.ConnectionStrings.ConnectionStrings["MongoDbDataConnection"]);
			Assert.True(pipeline3 is MongoDbEntityPipeline);
		}

		[Fact(DisplayName = "EntitySpider")]
		public void EntitySpiderRunCorrect()
		{
			CasSpider spider = new CasSpider();
			spider.Run();
		}

		[Schema("test", "table")]
		private class TestEntity : IBaseEntity
		{
			[Field(Expression = ".")]
			[Column]
			public string Name { get; set; }
		}

		private class ClearSchedulerSpider : EntitySpider
		{
			public ClearSchedulerSpider() : base("ClearSchedulerTestSpider")
			{
			}

			protected override void OnInit(params string[] arguments)
			{
				Monitor = new LogMonitor();
				Identity = Guid.NewGuid().ToString("N");
				//Scheduler = new RedisScheduler("127.0.0.1:6379,serviceName=Scheduler.NET,keepAlive=8,allowAdmin=True,connectTimeout=10000,password=,abortConnect=True,connectRetry=20");
				AddRequests("https://baidu.com");
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
				AddRequest(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word),
					new Dictionary<string, dynamic> {
						{ "Keyword", word },
						{ "guid", _guid }
					});
				AddEntityType<BaiduSearchEntry>();
			}

			[Schema("test", "baidu_search")]
			[Entity(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
			class BaiduSearchEntry : IBaseEntity
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

				[Column(0)]
				[Field(Expression = ".", Option = FieldOptions.InnerText)]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
				public string PlainText { get; set; }
			}
		}

		private class CasSpider : EntitySpider
		{
			public CasSpider() : base("casTest")
			{
			}

			protected override void OnInit(params string[] arguments)
			{
				Identity = Guid.NewGuid().ToString();
				EmptySleepTime = 5000;
				AddPipeline(new CollectionEntityPipeline());
				AddRequests("http://www.cas.cn/kx/kpwz/index.shtml");
				AddEntityType<ArticleSummary>();
			}

			[Entity(Expression = "//div[@class='ztlb_ld_mainR_box01_list']/ul/li")]
			class ArticleSummary : IBaseEntity
			{
				[Field(Expression = ".//a/@title")]
				public string Title { get; set; }

				[Next(Extras = new[] { "Title", "Url" })]
				[Field(Expression = ".//a/@href")]
				public string Url { get; set; }
			}
		}

		private class MultiEntitySpider : EntitySpider
		{
			protected override void OnInit(params string[] arguments)
			{
				EmptySleepTime = 6000;
				AddPipeline(new CollectionEntityPipeline());
				AddRequests("http://www.163.com");
				AddRequests("http://www.sohu.com");
				AddEntityType<NeteastEntity>();
				AddEntityType<SohuEntity>();
			}

			[Schema("test", "neteast")]
			[Target(Patterns = new[] { "http://www.163.com" })]
			public class NeteastEntity : IBaseEntity
			{
				[Field(Expression = ".//title")]
				[Column]
				public string Title { get; set; }
			}

			[Schema("test", "sohu")]
			[Target(Patterns = new[] { "http://www.sohu.com" })]
			public class SohuEntity : IBaseEntity
			{
				[Column]
				[Field(Expression = ".//title")]
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
