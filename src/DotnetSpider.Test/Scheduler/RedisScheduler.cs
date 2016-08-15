using System;
using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Scheduler;
using Xunit;

namespace DotnetSpider.Test.Scheduler
{
	public class RedisScheduler
	{
		[Fact]
		public void PushAndPoll1()
		{
			Extension.Scheduler.RedisScheduler scheduler = new Extension.Scheduler.RedisScheduler("localhost", "");

			ISpider spider = new DefaultSpider();
			scheduler.Init(spider);
			scheduler.Clear();

			Request request = new Request("http://www.ibm.com/developerworks/cn/java/j-javadev2-22/", 1, null);
			request.PutExtra("1", "2");
			scheduler.Push(request);
			Request result = scheduler.Poll();
			Assert.Equal("http://www.ibm.com/developerworks/cn/java/j-javadev2-22/", result.Url.ToString());
			Assert.Equal("2", request.GetExtra("1"));
			Request result1 = scheduler.Poll();
			Assert.Null(result1);
			scheduler.Dispose();
			scheduler.Clear();
		}

		[Fact]
		public void PushAndPoll2()
		{
			Extension.Scheduler.RedisScheduler scheduler = new Extension.Scheduler.RedisScheduler("localhost", "");

			ISpider spider = new DefaultSpider();
			scheduler.Init(spider);
			scheduler.Clear();
			Request request1 = new Request("http://www.ibm.com/1", 1, null);
			Request request2 = new Request("http://www.ibm.com/2", 1, null);
			Request request3 = new Request("http://www.ibm.com/3", 1, null);
			Request request4 = new Request("http://www.ibm.com/4", 1, null);
			scheduler.Push(request1);
			scheduler.Push(request2);
			scheduler.Push(request3);
			scheduler.Push(request4);

			Request result = scheduler.Poll();
			Assert.Equal("http://www.ibm.com/4", result.Url.ToString());
			Request result1 = scheduler.Poll();
			Assert.Equal("http://www.ibm.com/3", result1.Url.ToString());
			scheduler.Dispose();
			scheduler.Clear();
		}

		[Fact]
		public void LoadPerformace()
		{
			Extension.Scheduler.RedisScheduler scheduler = new Extension.Scheduler.RedisScheduler("localhost", "");
			ISpider spider = new DefaultSpider("test", new Core.Site());
			scheduler.Init(spider);
			scheduler.Clear();
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
			Assert.True(seconds1 < seconds);
			scheduler.Clear();
		}

		[Fact]
		public void Load()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			ISpider spider = new DefaultSpider("test", new Core.Site());
			scheduler.Init(spider);

			scheduler.Push(new Request("http://www.a.com/", 1, null));
			scheduler.Push(new Request("http://www.b.com/", 1, null));
			scheduler.Push(new Request("http://www.c.com/", 1, null));
			scheduler.Push(new Request("http://www.d.com/", 1, null));

			Extension.Scheduler.RedisScheduler redisScheduler = new Extension.Scheduler.RedisScheduler("localhost", "");
			redisScheduler.Init(spider);

			redisScheduler.Clear();

			redisScheduler.Load(scheduler.ToList());

			Assert.Equal("http://www.d.com/", redisScheduler.Poll().Url.ToString());
			Assert.Equal("http://www.c.com/", redisScheduler.Poll().Url.ToString());
			Assert.Equal("http://www.b.com/", redisScheduler.Poll().Url.ToString());
			Assert.Equal("http://www.a.com/", redisScheduler.Poll().Url.ToString());

			redisScheduler.Clear();
		}

		[Fact]
		public void Status()
		{
			Extension.Scheduler.RedisScheduler scheduler = new Extension.Scheduler.RedisScheduler("localhost", "");
			ISpider spider = new DefaultSpider("test", new Core.Site());
			scheduler.Init(spider);

			scheduler.Clear();

			scheduler.Push(new Request("http://www.a.com/", 1, null));
			scheduler.Push(new Request("http://www.b.com/", 1, null));
			scheduler.Push(new Request("http://www.c.com/", 1, null));
			scheduler.Push(new Request("http://www.d.com/", 1, null));

			Assert.Equal(0, scheduler.GetErrorRequestsCount());
			Assert.Equal(4,scheduler.GetLeftRequestsCount());
			Assert.Equal(4, scheduler.GetTotalRequestsCount());
			scheduler.IncreaseErrorCounter();
			Assert.Equal(1, scheduler.GetErrorRequestsCount());
			Assert.Equal(0, scheduler.GetSuccessRequestsCount());
			scheduler.IncreaseSuccessCounter();
			Assert.Equal(1, scheduler.GetSuccessRequestsCount());

			scheduler.Poll();
			Assert.Equal(3, scheduler.GetLeftRequestsCount());
			Assert.Equal(1, scheduler.GetSuccessRequestsCount());
			Assert.Equal(1, scheduler.GetErrorRequestsCount());
			Assert.Equal(4, scheduler.GetTotalRequestsCount());

			scheduler.Poll();
			Assert.Equal(2, scheduler.GetLeftRequestsCount());
			Assert.Equal(1, scheduler.GetSuccessRequestsCount());
			Assert.Equal(1, scheduler.GetErrorRequestsCount());
			Assert.Equal(4, scheduler.GetTotalRequestsCount());

			scheduler.Poll();
			Assert.Equal(1, scheduler.GetLeftRequestsCount());
			Assert.Equal(1, scheduler.GetSuccessRequestsCount());
			Assert.Equal(1, scheduler.GetErrorRequestsCount());
			Assert.Equal(4, scheduler.GetTotalRequestsCount());

			scheduler.Poll();
			Assert.Equal(0, scheduler.GetLeftRequestsCount());
			Assert.Equal(1, scheduler.GetSuccessRequestsCount());
			Assert.Equal(1, scheduler.GetErrorRequestsCount());
			Assert.Equal(4, scheduler.GetTotalRequestsCount());

			scheduler.Poll();
			scheduler.Poll();
			Assert.Equal(0, scheduler.GetLeftRequestsCount());
			Assert.Equal(1, scheduler.GetSuccessRequestsCount());
			Assert.Equal(1, scheduler.GetErrorRequestsCount());
			Assert.Equal(4, scheduler.GetTotalRequestsCount());

			scheduler.Clear();
		}

		[Fact]
		public void Clear()
		{
			Extension.Scheduler.RedisScheduler scheduler = new Extension.Scheduler.RedisScheduler("localhost", "");

			ISpider spider = new DefaultSpider();
			scheduler.Init(spider);
			scheduler.Clear();
			Request request1 = new Request("http://www.ibm.com/1", 1, null);
			Request request2 = new Request("http://www.ibm.com/2", 1, null);
			Request request3 = new Request("http://www.ibm.com/3", 1, null);
			Request request4 = new Request("http://www.ibm.com/4", 1, null);
			scheduler.Push(request1);
			scheduler.Push(request2);
			scheduler.Push(request3);
			scheduler.Push(request4);

			Request result = scheduler.Poll();
			Assert.Equal("http://www.ibm.com/4", result.Url.ToString());
 
			scheduler.Clear();
			scheduler.Dispose();
		}
	}
}