using System;
using System.Net;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using StackExchange.Redis;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Scheduler;
using Xunit;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Core.Selector;
using Dapper;
using System.Collections.Generic;
using DotnetSpider.Core.Infrastructure.Database;

namespace DotnetSpider.Extension.Test
{

	public class EntitySpiderTest
	{
		[Table("test", "table")]
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
				AddEntityType(typeof(TestEntity));
			}
		}

		[Fact]
		public void TestCorrectRedisSetting()
		{
			EntitySpider spider = new MyEntitySpider1();

			spider.Run("running-test");
		}

		//[Fact]
		//public void ThrowExceptionWhenNoEntity()
		//{
		//	try
		//	{
		//		EntitySpider spider = new EntitySpider(new Site());
		//		spider.Run("running-test");
		//	}
		//	catch (SpiderException exception)
		//	{
		//		Assert.Equal("Count of entity is zero.", exception.Message);
		//	}
		//}

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
				Identity = Guid.NewGuid().ToString("N");
				Scheduler = new RedisScheduler("127.0.0.1:6379,serviceName=Scheduler.NET,keepAlive=8,allowAdmin=True,connectTimeout=10000,password=,abortConnect=True,connectRetry=20");
				AddStartUrl("https://baidu.com");
				AddPipeline(new ConsoleEntityPipeline());
				AddEntityType(typeof(TestEntity));
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

			var md5 = Encrypt.Md5Encrypt(spider.Identity);
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
			var datetime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
			var guid = Guid.NewGuid().ToString();
			BaiduSearchSpider spider = new BaiduSearchSpider(guid);
			spider.Run();
			using (var conn = Core.Environment.DataConnectionStringSettings.GetDbConnection())
			{
				var count = conn.QueryFirst<int>($"SELECT COUNT(*) FROM test.baidu_search WHERE Guid='{guid}'");
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
			private string _guid;

			public BaiduSearchSpider(string guid) : base("BaiduSearch")
			{
				_guid = guid;
			}

			protected override void MyInit(params string[] arguments)
			{
				var word = "可乐|雪碧";
				Identity = Guid.NewGuid().ToString();
				AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word),
					new Dictionary<string, dynamic> {
						{ "Keyword", word },
						{ "guid", _guid }
					});
				AddEntityType(typeof(BaiduSearchEntry));
			}

			[Table("test", "baidu_search")]
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
			}
		}

		class CasSpider : EntitySpider
		{
			public CasSpider() : base("casTest", new Site())
			{
			}

			protected override void MyInit(params string[] arguments)
			{
				Identity = Guid.NewGuid().ToString();
				ThreadNum = 1;
				RetryWhenResultIsEmpty = false;
				Deep = 100;
				EmptySleepTime = 5000;
				ExitWhenComplete = true;
				CachedSize = 1;
				SkipWhenResultIsEmpty = true;
				SpawnUrl = true;
				AddPipeline(new CollectEntityPipeline());
				AddStartUrl("http://www.cas.cn/kx/kpwz/index.shtml");
				AddEntityType(typeof(ArticleSummary));
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
