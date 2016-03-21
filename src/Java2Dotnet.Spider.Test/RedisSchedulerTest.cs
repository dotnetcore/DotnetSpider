using System.Collections.Generic;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Scheduler;
#if !NET_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Java2Dotnet.Spider.Test
{
	[TestClass]
	public class RedisSchedulerTest
	{
		[TestMethod]
		public void RedisTest()
		{
			RedisScheduler redisScheduler = new RedisScheduler("localhost", "");

			ISpider spider = new TestSpider();
			Request request = new Request("http://www.ibm.com/developerworks/cn/java/j-javadev2-22/", 1, null);
			request.PutExtra("1", "2");
			redisScheduler.Push(request, spider);
			Request result = redisScheduler.Poll(spider);
			Assert.AreEqual("http://www.ibm.com/developerworks/cn/java/j-javadev2-22/", result.Url.ToString());
			Request result1 = redisScheduler.Poll(spider);
			Assert.IsNull(result1);
			redisScheduler.Dispose();

			RedisSchedulerManager m = new RedisSchedulerManager("localhost");
			m.RemoveTask(spider.Identity);
		}

		[TestMethod]
		public void Redis_QueueTest()
		{
			RedisScheduler redisScheduler = new RedisScheduler("localhost", "");

			ISpider spider = new TestSpider();
			Request request1 = new Request("http://www.ibm.com/1", 1, null);
			Request request2 = new Request("http://www.ibm.com/2", 1, null);
			Request request3 = new Request("http://www.ibm.com/3", 1, null);
			Request request4 = new Request("http://www.ibm.com/4", 1, null);
			redisScheduler.Push(request1, spider);
			redisScheduler.Push(request2, spider);
			redisScheduler.Push(request3, spider);
			redisScheduler.Push(request4, spider);

			Request result = redisScheduler.Poll(spider);
			Assert.AreEqual("http://www.ibm.com/4", result.Url.ToString());
			Request result1 = redisScheduler.Poll(spider);
			Assert.AreEqual("http://www.ibm.com/3", result1.Url.ToString());
			redisScheduler.Dispose();

			RedisSchedulerManager m = new RedisSchedulerManager("localhost");
			m.RemoveTask(spider.Identity);
		}
	}

	internal class TestSpider : ISpider
	{
		public string Identity => "1";

		public Site Site => null;
		public void Start()
		{
		}

		public void Run()
		{
		}

		public void Stop()
		{
		}

		public Dictionary<string, dynamic> Settings { get; } = new Dictionary<string, dynamic>();

		public void Exit()
		{

		}

		public void Dispose()
		{
		}
	}
}