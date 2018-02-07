using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Infrastructure.Database;
using DotnetSpider.Core.Monitor;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Scheduler;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace DotnetSpider.Extension.Test
{

	public class EntitySpiderTest
	{
		public EntitySpiderTest()
		{
			Env.EnterpiseService = false;
		}

		[EntityTable("test", "table")]
		public class TestEntity : SpiderEntity
		{
			[PropertyDefine(Expression = ".")]
			public string Name { get; set; }
		}

		public class MyEntitySpider1 : EntitySpider
		{
			public MyEntitySpider1() : base("tes", new Site())
			{
			}

			protected override void MyInit(params string[] arguments)
			{
				Identity = Guid.NewGuid().ToString();
				AddPipeline(new ConsoleEntityPipeline());
				AddEntityType<TestEntity>();
			}
		}

		[Fact]
		public void TestCorrectRedisSetting()
		{
			EntitySpider spider = new MyEntitySpider1();

			spider.Run("running-test");
		}

		[Fact]
		public void MultiEntity()
		{
			EntitySpider spider = new MultiEntitySpider();
			spider.Run();
		}

		private class MultiEntitySpider : EntitySpider
		{
			protected override void MyInit(params string[] arguments)
			{
				Monitor = new NLogMonitor();
				AddStartUrl("http://www.baidu.com");
				AddStartUrl("http://www.sohu.com");
				AddEntityType<BaiduEntity>();
				AddEntityType<SohuEntity>();
			}

			[EntityTable("test", "multy_entity")]
			[TargetUrlsSelector(XPaths = new string[] { }, Patterns = new[] { "baidu" })]
			public class BaiduEntity : SpiderEntity
			{
				[PropertyDefine(Expression = ".//title")]
				public string Title { get; set; }
			}

			[EntityTable("test", "multy_entity")]
			[TargetUrlsSelector(XPaths = new string[] { }, Patterns = new[] { "sohu" })]
			public class SohuEntity : SpiderEntity
			{
				[PropertyDefine(Expression = ".//title")]
				public string Title { get; set; }
			}
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

		public class ClearSchedulerTestSpider : EntitySpider
		{
			public ClearSchedulerTestSpider() : base("ClearSchedulerTestSpider", new Site())
			{
			}

			protected override void MyInit(params string[] arguments)
			{
				Monitor = new NLogMonitor();
				Identity = Guid.NewGuid().ToString("N");
				Scheduler = new RedisScheduler("127.0.0.1:6379,serviceName=Scheduler.NET,keepAlive=8,allowAdmin=True,connectTimeout=10000,password=,abortConnect=True,connectRetry=20");
				AddStartUrl("https://baidu.com");
				AddPipeline(new ConsoleEntityPipeline());
				AddEntityType<TestEntity>();
			}
		}

		[Fact]
		public void ClearScheduler()
		{
			EntitySpider spider = new ClearSchedulerTestSpider();

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

			var md5 = CryptoUtil.Md5Encrypt(spider.Identity);
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

		[Fact]
		public void EntitySpiderWithDefaultPipeline()
		{
			var guid = Guid.NewGuid().ToString();
			BaiduSearchSpider spider = new BaiduSearchSpider(guid);
			spider.Run();
			using (var conn = Env.DataConnectionStringSettings.CreateDbConnection())
			{
				var count = conn.QueryFirst<int>($"SELECT COUNT(*) FROM test.baidu_search WHERE `guid`='{guid}'");
				Assert.Equal(20, count);
			}
		}

		[Fact]
		public void EntitySpiderRunCorrect()
		{
			CasSpider spider = new CasSpider();
			spider.Run();
		}

		class BaiduSearchSpider : EntitySpider
		{
			private readonly string _guid;

			public BaiduSearchSpider(string guid) : base("BaiduSearch")
			{
				_guid = guid;
			}

			protected override void MyInit(params string[] arguments)
			{
				Monitor = new NLogMonitor();
				var word = "可乐|雪碧";
				Identity = Guid.NewGuid().ToString();
				AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word),
					new Dictionary<string, dynamic> {
						{ "Keyword", word },
						{ "guid", _guid }
					});
				AddPipeline(new MySqlEntityPipeline { DefaultPipelineModel = PipelineMode.Insert });
				AddEntityType<BaiduSearchEntry>();
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

		class CasSpider : EntitySpider
		{
			public CasSpider() : base("casTest", new Site())
			{
			}

			protected override void MyInit(params string[] arguments)
			{
				Monitor = new NLogMonitor();
				Identity = Guid.NewGuid().ToString();
				ThreadNum = 1;
				Deep = 100;
				EmptySleepTime = 5000;
				ExitWhenComplete = true;
				CachedSize = 1;
				SkipWhenResultIsEmpty = false;
				AddPipeline(new CollectionEntityPipeline());
				AddStartUrl("http://www.cas.cn/kx/kpwz/index.shtml");
				AddEntityType<ArticleSummary>();
			}

			[EntitySelector(Expression = "//div[@class='ztlb_ld_mainR_box01_list']/ul/li")]
			class ArticleSummary : SpiderEntity
			{
				[PropertyDefine(Expression = ".//a/@title")]
				public string Title { get; set; }

				[LinkToNext(Extras = new[] { "Title", "Url" })]
				[PropertyDefine(Expression = ".//a/@href")]
				public string Url { get; set; }
			}
		}

		private static void ClearDb()
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
