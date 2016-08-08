using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Extension.Scheduler;
using DotnetSpider.Core.Scheduler;
using System;
#if !NET_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace DotnetSpider.Test
{
	[TestClass]
	public class RedisSchedulerTest
	{
		[TestMethod]
		public void RedisTest()
		{

			RedisScheduler redisScheduler = new RedisScheduler("localhost", "");

			ISpider spider = new DefaultSpider();
			redisScheduler.Clear();

			Request request = new Request("http://www.ibm.com/developerworks/cn/java/j-javadev2-22/", 1, null);
			request.PutExtra("1", "2");
			redisScheduler.Push(request);
			Request result = redisScheduler.Poll();
			Assert.AreEqual("http://www.ibm.com/developerworks/cn/java/j-javadev2-22/", result.Url.ToString());
			Request result1 = redisScheduler.Poll();
			Assert.IsNull(result1);
			redisScheduler.Dispose();

			redisScheduler.Clear();
		}

		[TestMethod]
		public void Redis_QueueTest()
		{
			RedisScheduler redisScheduler = new RedisScheduler("localhost", "");

			ISpider spider = new DefaultSpider();
			Request request1 = new Request("http://www.ibm.com/1", 1, null);
			Request request2 = new Request("http://www.ibm.com/2", 1, null);
			Request request3 = new Request("http://www.ibm.com/3", 1, null);
			Request request4 = new Request("http://www.ibm.com/4", 1, null);
			redisScheduler.Push(request1);
			redisScheduler.Push(request2);
			redisScheduler.Push(request3);
			redisScheduler.Push(request4);

			Request result = redisScheduler.Poll();
			Assert.AreEqual("http://www.ibm.com/4", result.Url.ToString());
			Request result1 = redisScheduler.Poll();
			Assert.AreEqual("http://www.ibm.com/3", result1.Url.ToString());
			redisScheduler.Dispose();
		}

		[TestMethod]
		public void SchedulerLoadPerformaceTest()
		{
			RedisScheduler scheduler = new RedisScheduler("localhost", "");
			ISpider spider = new DefaultSpider("test", new Site());
			var start = DateTime.Now;
			for (int i = 0; i < 40000; i++)
			{
				scheduler.Push(new Request("http://www.a.com/" + i, 1, null));
			}

			var end = DateTime.Now;
			double seconds = (end - start).TotalSeconds;
			scheduler.Clear();

			var start1 = DateTime.Now;
			HashSet<Request> list = new HashSet<Request>();
			for (int i = 0; i < 40000; i++)
			{
				list.Add(new Request("http://www.a.com/" + i, 1, null));
			}
			scheduler.Load(list);
			var end1 = DateTime.Now;
			double seconds1 = (end1 - start1).TotalSeconds;
			Assert.IsTrue(seconds1 < seconds);
		}

		[TestMethod]
		public void SchedulerLoadTest()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			ISpider spider = new DefaultSpider("test", new Site());
			scheduler.Push(new Request("http://www.a.com/", 1, null));
			scheduler.Push(new Request("http://www.b.com/", 1, null));
			scheduler.Push(new Request("http://www.c.com/", 1, null));
			scheduler.Push(new Request("http://www.d.com/", 1, null));

			RedisScheduler redisScheduler = new RedisScheduler("localhost", "");
			redisScheduler.Load(scheduler.ToList());

			Assert.AreEqual("http://www.d.com/", redisScheduler.Poll().Url.ToString());
			Assert.AreEqual("http://www.c.com/", redisScheduler.Poll().Url.ToString());
			Assert.AreEqual("http://www.b.com/", redisScheduler.Poll().Url.ToString());
			Assert.AreEqual("http://www.a.com/", redisScheduler.Poll().Url.ToString());
		}
	}
}