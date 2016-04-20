using System.Collections.Generic;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Scheduler;
using Java2Dotnet.Spider.Core.Scheduler;
using System;
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
			RedisSchedulerManager m = new RedisSchedulerManager("localhost");
			m.RemoveTask(spider.Identity);

			Request request = new Request("http://www.ibm.com/developerworks/cn/java/j-javadev2-22/", 1, null);
			request.PutExtra("1", "2");
			redisScheduler.Push(request, spider);
			Request result = redisScheduler.Poll(spider);
			Assert.AreEqual("http://www.ibm.com/developerworks/cn/java/j-javadev2-22/", result.Url.ToString());
			Request result1 = redisScheduler.Poll(spider);
			Assert.IsNull(result1);
			redisScheduler.Dispose();


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

		[TestMethod]
		public void SchedulerLoadPerformaceTest()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			ISpider spider = new DefaultSpider("test", new Site());
			var start = DateTime.Now;
			for (int i = 0; i < 40000; i++)
			{
				scheduler.Push(new Request("http://www.a.com/" + i, 1, null), spider);
			}

			RedisScheduler redisScheduler = new RedisScheduler("localhost", "");
			redisScheduler.Load(scheduler.ToList(spider), spider);
			var end = DateTime.Now;
			double seconds = (end - start).TotalSeconds;
			redisScheduler.Clear(spider);

			var start1 = DateTime.Now;
			for (int i = 0; i < 40000; i++)
			{
				redisScheduler.Push(new Request("http://www.a.com/" + i, 1, null), spider);
			}
			var end1 = DateTime.Now;
			double seconds1 = (end1 - start1).TotalSeconds;
			Assert.IsTrue((seconds1 / seconds) > 6);
		}

		[TestMethod]
		public void SchedulerLoadTest()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			ISpider spider = new DefaultSpider("test", new Site());
			scheduler.Push(new Request("http://www.a.com/", 1, null), spider);
			scheduler.Push(new Request("http://www.b.com/", 1, null), spider);
			scheduler.Push(new Request("http://www.c.com/", 1, null), spider);
			scheduler.Push(new Request("http://www.d.com/", 1, null), spider);

			RedisScheduler redisScheduler = new RedisScheduler("localhost", "");
			redisScheduler.Load(scheduler.ToList(spider), spider);

			Assert.AreEqual("http://www.d.com/", redisScheduler.Poll(spider).Url.ToString());
			Assert.AreEqual("http://www.c.com/", redisScheduler.Poll(spider).Url.ToString());
			Assert.AreEqual("http://www.b.com/", redisScheduler.Poll(spider).Url.ToString());
			Assert.AreEqual("http://www.a.com/", redisScheduler.Poll(spider).Url.ToString());
		}
	}

	internal class TestSpider : ISpider
	{
		public string Identity => "1";

		public Site Site => new Site { EncodingName = "UTF-8" };
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