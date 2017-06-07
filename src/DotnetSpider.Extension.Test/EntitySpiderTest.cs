using System;
using System.Net;
using System.Threading;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using StackExchange.Redis;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Extension.Test
{
	[TestClass]
	public class EntitySpiderTest
	{
		[Table("test", "table")]
		public class TestEntity : SpiderEntity
		{

		}

		public class MyEntitySpider1 : EntitySpider
		{
			public MyEntitySpider1(Site site) : base(site)
			{
				//RedisHost = "redis";
				//RedisPassword = "test";
			}
		}

		[TestMethod]
		public void TestCorrectRedisSetting()
		{
			EntitySpider spider = new EntitySpider(new Site());
			spider.AddPipeline(new ConsoleEntityPipeline());
			spider.AddEntityType(typeof(TestEntity));
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

		[TestMethod]
		public void ClearScheduler()
		{
			EntitySpider spider = new EntitySpider(new Site());
			spider.Identity = Guid.NewGuid().ToString("N");
			spider.SetScheduler(new RedisScheduler("127.0.0.1:6379,serviceName=Scheduler.NET,keepAlive=8,allowAdmin=True,connectTimeout=10000,password=6GS9F2QTkP36GggE0c3XwVwI,abortConnect=True,connectRetry=20"));
			spider.AddStartUrl("https://baidu.com");
			spider.AddPipeline(new ConsoleEntityPipeline());
			spider.AddEntityType(typeof(TestEntity));
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

		public class CasSpider : EntitySpiderBuilder
		{
			public CasSpider() : base("cas", Batch.Now)
			{
			}

			protected override EntitySpider GetEntitySpider()
			{
				EntitySpider context = new EntitySpider(new Site());
				context.SetSite(new Site());
				context.SetThreadNum(2);
				context.ThreadNum = 1;
				context.RetryWhenResultIsEmpty = false;
				context.Deep = 100;
				context.EmptySleepTime = 5000;
				context.SetEmptySleepTime(5000);
				context.ExitWhenComplete = true;
				context.CachedSize = 1;
				context.SetDownloader(new HttpClientDownloader());
				context.SetScheduler(new QueueDuplicateRemovedScheduler());

				context.SkipWhenResultIsEmpty = true;
				context.SpawnUrl = true;
				context.AddPipeline(new CollectEntityPipeline());
				context.AddStartUrl("http://www.cas.cn/kx/kpwz/index.shtml");
				context.AddEntityType(typeof(ArticleSummary));
				return context;
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
