using System;
using DotnetSpider.Core;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using StackExchange.Redis;
using Xunit;

namespace DotnetSpider.Test
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
		public void TestInCorrectRedisSetting()
		{
			try
			{
				MyEntitySpider1 spider = new MyEntitySpider1(new Site());
				spider.AddEntityPipeline(new ConsoleEntityPipeline());
				spider.AddEntityType(typeof(TestEntity));
				spider.Run("running-test");
			}
			catch (RedisConnectionException)
			{
				return;
			}
			throw new Exception("TEST FAILED.");
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
	}
}
