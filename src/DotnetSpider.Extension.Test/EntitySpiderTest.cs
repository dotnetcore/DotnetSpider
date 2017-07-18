using System;
using System.Net;
using System.Threading;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using StackExchange.Redis;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Extension.Test
{
	[TestClass]
	public class EntitySpiderTest
	{
		[Table("test", "table")]
		public class TestEntity : SpiderEntity
		{
			[PropertyDefine(Expression = ".")]
			public string name { get; set; }
		}

		public class MyEntitySpider1 : EntitySpider
		{
			public MyEntitySpider1() : base("tes")
			{
			}

			protected override void MyInit(params string[] arguments)
			{
				Identity = Guid.NewGuid().ToString();
				AddPipeline(new ConsoleEntityPipeline());
				AddEntityType(typeof(TestEntity));
			}
		}

		[TestMethod]
		public void TestCorrectRedisSetting()
		{
			EntitySpider spider = new MyEntitySpider1();

			spider.Run("running-test");
		}

		//[TestMethod]
		//public void ThrowExceptionWhenNoEntity()
		//{
		//	try
		//	{
		//		EntitySpider spider = new EntitySpider(new Site());
		//		spider.Run("running-test");
		//	}
		//	catch (SpiderException exception)
		//	{
		//		Assert.AreEqual("Count of entity is zero.", exception.Message);
		//	}
		//}

		[TestMethod]
		public void RedisKeepConnect()
		{
			var confiruation = new ConfigurationOptions()
			{
				ServiceName = "DotnetSpider",
				ConnectTimeout = 65530,
				KeepAlive = 8,
				ConnectRetry = 3,
				ResponseTimeout = 3000,
				Password = "6GS9F2QTkP36GggE0c3XwVwI",
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


		public class ClearSchedulerTestSpider : EntitySpider
		{
			public ClearSchedulerTestSpider() : base("ClearSchedulerTestSpider")
			{
			}

			protected override void MyInit(params string[] arguments)
			{
				Identity = Guid.NewGuid().ToString("N");
				SetScheduler(new RedisScheduler("127.0.0.1:6379,serviceName=Scheduler.NET,keepAlive=8,allowAdmin=True,connectTimeout=10000,password=6GS9F2QTkP36GggE0c3XwVwI,abortConnect=True,connectRetry=20"));
				AddStartUrl("https://baidu.com");
				AddPipeline(new ConsoleEntityPipeline());
				AddEntityType(typeof(TestEntity));
			}
		}

		[TestMethod]
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
				Password = "6GS9F2QTkP36GggE0c3XwVwI",
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
			Assert.AreEqual(0, db.ListLength(queueKey));
			//set
			Assert.AreEqual(0, db.SetLength(setKey));
			//item
			Assert.AreEqual(0, db.HashLength(itemKey));
			//error-count
			Assert.AreEqual(false, db.StringGet(errorCountKey).HasValue);
			//success-count
			Assert.AreEqual(false, db.StringGet(successCountKey).HasValue);
		}

		[TestMethod]
		public void Run()
		{
			CasSpider spider = new CasSpider();
			spider.Run();
		}

		public class CasSpider : EntitySpider
		{
			public CasSpider() : base("casTest")
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
			public class ArticleSummary : SpiderEntity
			{
				[PropertyDefine(Expression = ".//a/@title")]
				public string Title { get; set; }

				[LinkToNext(Extras = new[] { "Title", "Url" })]
				[PropertyDefine(Expression = ".//a/@href")]
				public string Url { get; set; }
			}
		}

		public static void ClearDB()
		{
			var confiruation = new ConfigurationOptions()
			{
				ServiceName = "DotnetSpider",
				ConnectTimeout = 65530,
				KeepAlive = 8,
				ConnectRetry = 3,
				ResponseTimeout = 3000,
				Password = "6GS9F2QTkP36GggE0c3XwVwI",
				AllowAdmin = true
			};

			confiruation.EndPoints.Add(new DnsEndPoint("127.0.0.1", 6379));

			var redis = ConnectionMultiplexer.Connect(confiruation);
			var server = redis.GetServer(redis.GetEndPoints()[0]);
			server.FlushAllDatabases();
		}
	}
}
