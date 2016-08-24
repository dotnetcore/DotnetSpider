using DotnetSpider.Extension;
using DotnetSpider.Extension.Pipeline;
using StackExchange.Redis;
using Xunit;

namespace DotnetSpider.Test
{
	public class EntitySpiderTest
	{
		public class MyEntitySpider1 : EntitySpider
		{
			public MyEntitySpider1(Core.Site site) : base(site)
			{
				RedisHost = "redis";
				RedisPassword = "test";
			}
		}

		[Fact]
		public void TestInCorrectRedisSetting()
		{
			Assert.Throws<RedisConnectionException>(() =>
			{
				MyEntitySpider1 spider = new MyEntitySpider1(new Core.Site());
				spider.Run("running-test");
			});
		}

		[Fact]
		public void TestCorrectRedisSetting()
		{
			EntitySpider spider = new EntitySpider(new Core.Site());
			spider.AddEntityPipeline(new ConsoleEntityPipeline());
			spider.Run("running-test");
		}

		[Fact]
		public void ThrowExceptionWhenNoPipeline()
		{
			var exception = Assert.Throws<Core.SpiderException>(() =>
			{
				EntitySpider spider = new EntitySpider(new Core.Site());
				spider.Run("running-test");
			});
			Assert.Equal("Pipelines should not be null.", exception.Message);
		}
	}
}
