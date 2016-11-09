using System;
using System.Net;
using System.Threading;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using StackExchange.Redis;
using Xunit;

namespace DotnetSpider.Extension.Test
{
	public class EntitySpiderTest
	{
		[Schema("test", "table")]
		public class TestEntity : ISpiderEntity
		{

		}

		public class MyEntitySpider1 : EntitySpider
		{
			public MyEntitySpider1(Site site) : base(site)
			{
				RedisHost = "redis";
				RedisPassword = "test";
			}
		}

		[Fact]
		public void TestCorrectRedisSetting()
		{
			EntitySpider spider = new EntitySpider(new Site());
			spider.AddEntityPipeline(new ConsoleEntityPipeline());
			spider.AddEntityType(typeof(TestEntity));
			spider.Run("running-test");
		}

		[Fact]
		public void ThrowExceptionWhenNoEntity()
		{
			try
			{
				EntitySpider spider = new EntitySpider(new Site());
				spider.Run("running-test");
			}
			catch (SpiderException exception)
			{
				Assert.Equal("Count of entity is 0.", exception.Message);
			}
		}

		[Fact]
		public void ThrowExceptionWhenNoEntityPipeline()
		{
			try
			{
				EntitySpider spider = new EntitySpider(new Site());
				spider.AddEntityType(typeof(TestEntity));
				spider.Run("running-test");
			}
			catch (SpiderException exception)
			{
				Assert.Equal("Need at least one entity pipeline.", exception.Message);
			}
		}

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
				Password = "6GS9F2QTkP36GggE0c3XwVwI"
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
	}
}
