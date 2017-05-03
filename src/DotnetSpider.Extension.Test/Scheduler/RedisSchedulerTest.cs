using System;
using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetSpider.Extension.Test.Scheduler
{
	[TestClass]
	public class RedisSchedulerTest
	{
		private Extension.Scheduler.RedisScheduler GetRedisScheduler()
		{
			return new Extension.Scheduler.RedisScheduler("127.0.0.1:6379,serviceName=Scheduler.NET,keepAlive=8,allowAdmin=True,connectTimeout=10000,password=6GS9F2QTkP36GggE0c3XwVwI,abortConnect=True,connectRetry=20");
		}

		[TestMethod]
		public void PushAndPoll1()
		{
			Extension.Scheduler.RedisScheduler scheduler = GetRedisScheduler();

			ISpider spider = new DefaultSpider();
			scheduler.Init(spider);
			scheduler.Dispose();

			Request request = new Request("http://www.ibm.com/developerworks/cn/java/j-javadev2-22/", null);
			request.PutExtra("1", "2");
			scheduler.Push(request);
			Request result = scheduler.Poll();
			Assert.AreEqual("http://www.ibm.com/developerworks/cn/java/j-javadev2-22/", result.Url.ToString());
			Assert.AreEqual("2", request.GetExtra("1"));
			Request result1 = scheduler.Poll();
			Assert.IsNull(result1);
			scheduler.Dispose();
		}

		[TestMethod]
		public void PushAndPollBreadthFirst()
		{
			Extension.Scheduler.RedisScheduler scheduler = GetRedisScheduler();
			scheduler.DepthFirst = false;
			ISpider spider = new DefaultSpider();
			scheduler.Init(spider);
			scheduler.Dispose();
			Request request1 = new Request("http://www.ibm.com/1", null);
			Request request2 = new Request("http://www.ibm.com/2", null);
			Request request3 = new Request("http://www.ibm.com/3", null);
			Request request4 = new Request("http://www.ibm.com/4", null);
			scheduler.Push(request1);
			scheduler.Push(request2);
			scheduler.Push(request3);
			scheduler.Push(request4);

			Request result = scheduler.Poll();
			Assert.AreEqual("http://www.ibm.com/1", result.Url.ToString());
			Request result1 = scheduler.Poll();
			Assert.AreEqual("http://www.ibm.com/2", result1.Url.ToString());
			scheduler.Dispose();
			scheduler.Dispose();
		}

		[TestMethod]
		public void PushAndPollDepthFirst()
		{
			Extension.Scheduler.RedisScheduler scheduler = GetRedisScheduler();
			scheduler.DepthFirst = true;
			ISpider spider = new DefaultSpider();
			scheduler.Init(spider);
			scheduler.Dispose();
			Request request1 = new Request("http://www.ibm.com/1", null);
			Request request2 = new Request("http://www.ibm.com/2", null);
			Request request3 = new Request("http://www.ibm.com/3", null);
			Request request4 = new Request("http://www.ibm.com/4", null);
			scheduler.Push(request1);
			scheduler.Push(request2);
			scheduler.Push(request3);
			scheduler.Push(request4);

			Request result = scheduler.Poll();
			Assert.AreEqual("http://www.ibm.com/4", result.Url.ToString());
			Request result1 = scheduler.Poll();
			Assert.AreEqual("http://www.ibm.com/3", result1.Url.ToString());
			scheduler.Dispose();
			scheduler.Dispose();
		}

		[TestMethod]
		public void LoadPerformace()
		{
			Extension.Scheduler.RedisScheduler scheduler = GetRedisScheduler();
			ISpider spider = new DefaultSpider("test", new Site());
			scheduler.Init(spider);
			scheduler.Dispose();
			var start = DateTime.Now;
			for (int i = 0; i < 40000; i++)
			{
				scheduler.Push(new Request("http://www.a.com/" + i, null));
			}

			var end = DateTime.Now;
			double seconds = (end - start).TotalSeconds;
			scheduler.Dispose();

			var start1 = DateTime.Now;
			HashSet<Request> list = new HashSet<Request>();
			for (int i = 0; i < 40000; i++)
			{
				list.Add(new Request("http://www.a.com/" + i, null));
			}
			scheduler.Import(list);
			var end1 = DateTime.Now;
			double seconds1 = (end1 - start1).TotalSeconds;
			Assert.IsTrue(seconds1 < seconds);
			scheduler.Dispose();
		}

		[TestMethod]
		public void Load()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			ISpider spider = new DefaultSpider("test", new Site());
			scheduler.Init(spider);

			scheduler.Push(new Request("http://www.a.com/", null));
			scheduler.Push(new Request("http://www.b.com/", null));
			scheduler.Push(new Request("http://www.c.com/", null));
			scheduler.Push(new Request("http://www.d.com/", null));

			Extension.Scheduler.RedisScheduler redisScheduler = GetRedisScheduler();
			redisScheduler.Init(spider);

			redisScheduler.Dispose();

			redisScheduler.Import(scheduler.ToList());

			Assert.AreEqual("http://www.d.com/", redisScheduler.Poll().Url.ToString());
			Assert.AreEqual("http://www.c.com/", redisScheduler.Poll().Url.ToString());
			Assert.AreEqual("http://www.b.com/", redisScheduler.Poll().Url.ToString());
			Assert.AreEqual("http://www.a.com/", redisScheduler.Poll().Url.ToString());

			redisScheduler.Dispose();
		}

		[TestMethod]
		public void Status()
		{
			Extension.Scheduler.RedisScheduler scheduler = GetRedisScheduler();
			ISpider spider = new DefaultSpider("test", new Site());
			scheduler.Init(spider);

			scheduler.Clean();

			scheduler.Push(new Request("http://www.a.com/", null));
			scheduler.Push(new Request("http://www.b.com/", null));
			scheduler.Push(new Request("http://www.c.com/", null));
			scheduler.Push(new Request("http://www.d.com/", null));

			Assert.AreEqual(0, scheduler.GetErrorRequestsCount());
			Assert.AreEqual(4, scheduler.GetLeftRequestsCount());
			Assert.AreEqual(4, scheduler.GetTotalRequestsCount());
			scheduler.IncreaseErrorCounter();
			Assert.AreEqual(1, scheduler.GetErrorRequestsCount());
			Assert.AreEqual(0, scheduler.GetSuccessRequestsCount());
			scheduler.IncreaseSuccessCounter();
			Assert.AreEqual(1, scheduler.GetSuccessRequestsCount());

			scheduler.Poll();
			Assert.AreEqual(3, scheduler.GetLeftRequestsCount());
			Assert.AreEqual(1, scheduler.GetSuccessRequestsCount());
			Assert.AreEqual(1, scheduler.GetErrorRequestsCount());
			Assert.AreEqual(4, scheduler.GetTotalRequestsCount());

			scheduler.Poll();
			Assert.AreEqual(2, scheduler.GetLeftRequestsCount());
			Assert.AreEqual(1, scheduler.GetSuccessRequestsCount());
			Assert.AreEqual(1, scheduler.GetErrorRequestsCount());
			Assert.AreEqual(4, scheduler.GetTotalRequestsCount());

			scheduler.Poll();
			Assert.AreEqual(1, scheduler.GetLeftRequestsCount());
			Assert.AreEqual(1, scheduler.GetSuccessRequestsCount());
			Assert.AreEqual(1, scheduler.GetErrorRequestsCount());
			Assert.AreEqual(4, scheduler.GetTotalRequestsCount());

			scheduler.Poll();
			Assert.AreEqual(0, scheduler.GetLeftRequestsCount());
			Assert.AreEqual(1, scheduler.GetSuccessRequestsCount());
			Assert.AreEqual(1, scheduler.GetErrorRequestsCount());
			Assert.AreEqual(4, scheduler.GetTotalRequestsCount());

			scheduler.Poll();
			scheduler.Poll();
			Assert.AreEqual(0, scheduler.GetLeftRequestsCount());
			Assert.AreEqual(1, scheduler.GetSuccessRequestsCount());
			Assert.AreEqual(1, scheduler.GetErrorRequestsCount());
			Assert.AreEqual(4, scheduler.GetTotalRequestsCount());

			scheduler.Clean();
		}

		[TestMethod]
		public void MultiInit()
		{
			Extension.Scheduler.RedisScheduler scheduler = GetRedisScheduler();

			ISpider spider = new DefaultSpider();
			scheduler.Init(spider);
			string queueKey = scheduler.GetQueueKey();
			string setKey = scheduler.GetSetKey();
			string itemKey = scheduler.GetItemKey();
			string errorCountKey = scheduler.GetErrorCountKey();
			string successCountKey = scheduler.GetSuccessCountKey();
			scheduler.Init(spider);
			Assert.AreEqual(queueKey, scheduler.GetQueueKey());
			Assert.AreEqual(setKey, scheduler.GetSetKey());
			Assert.AreEqual(itemKey, scheduler.GetItemKey());
			Assert.AreEqual(errorCountKey, scheduler.GetErrorCountKey());
			Assert.AreEqual(successCountKey, scheduler.GetSuccessCountKey());

			scheduler.Dispose();
			scheduler.Dispose();
		}

		[TestMethod]
		public void Clear()
		{
			Extension.Scheduler.RedisScheduler scheduler = GetRedisScheduler();

			ISpider spider = new DefaultSpider();
			scheduler.Init(spider);
			scheduler.Dispose();
			Request request1 = new Request("http://www.ibm.com/1", null);
			Request request2 = new Request("http://www.ibm.com/2", null);
			Request request3 = new Request("http://www.ibm.com/3", null);
			Request request4 = new Request("http://www.ibm.com/4", null);
			scheduler.Push(request1);
			scheduler.Push(request2);
			scheduler.Push(request3);
			scheduler.Push(request4);

			Request result = scheduler.Poll();
			Assert.AreEqual("http://www.ibm.com/4", result.Url.ToString());

			scheduler.Dispose();
		}
	}
}