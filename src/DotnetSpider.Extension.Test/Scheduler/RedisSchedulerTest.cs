using System;
using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Scheduler;
using Xunit;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;

namespace DotnetSpider.Extension.Test.Scheduler
{
	
	public class RedisSchedulerTest
	{
		private Extension.Scheduler.RedisScheduler GetRedisScheduler()
		{
			return new Extension.Scheduler.RedisScheduler("127.0.0.1:6379,serviceName=Scheduler.NET,keepAlive=8,allowAdmin=True,connectTimeout=10000,password=6GS9F2QTkP36GggE0c3XwVwI,abortConnect=True,connectRetry=20");
		}

		[Fact]
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
			Assert.Equal("http://www.ibm.com/developerworks/cn/java/j-javadev2-22/", result.Url.ToString());
			Assert.Equal("2", request.GetExtra("1"));
			Request result1 = scheduler.Poll();
			Assert.Null(result1);
			scheduler.Dispose();
		}

		[Fact]
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
			Assert.Equal("http://www.ibm.com/1", result.Url.ToString());
			Request result1 = scheduler.Poll();
			Assert.Equal("http://www.ibm.com/2", result1.Url.ToString());
			scheduler.Dispose();
			scheduler.Dispose();
		}

		[Fact]
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
			Assert.Equal("http://www.ibm.com/4", result.Url.ToString());
			Request result1 = scheduler.Poll();
			Assert.Equal("http://www.ibm.com/3", result1.Url.ToString());
			scheduler.Dispose();
			scheduler.Dispose();
		}

		[Fact]
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
			Assert.True(seconds1 < seconds);
			scheduler.Dispose();
		}

		[Fact]
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

			Assert.Equal("http://www.d.com/", redisScheduler.Poll().Url.ToString());
			Assert.Equal("http://www.c.com/", redisScheduler.Poll().Url.ToString());
			Assert.Equal("http://www.b.com/", redisScheduler.Poll().Url.ToString());
			Assert.Equal("http://www.a.com/", redisScheduler.Poll().Url.ToString());

			redisScheduler.Dispose();
		}

		[Fact]
		public void Status()
		{
			Extension.Scheduler.RedisScheduler scheduler = GetRedisScheduler();
			ISpider spider = new DefaultSpider("test", new Site());
			scheduler.Init(spider);

			scheduler.Clear();

			scheduler.Push(new Request("http://www.a.com/", null));
			scheduler.Push(new Request("http://www.b.com/", null));
			scheduler.Push(new Request("http://www.c.com/", null));
			scheduler.Push(new Request("http://www.d.com/", null));

			Assert.Equal(0, scheduler.ErrorRequestsCount);
			Assert.Equal(4, scheduler.LeftRequestsCount);
			Assert.Equal(4, scheduler.TotalRequestsCount);
			scheduler.IncreaseErrorCount();
			Assert.Equal(1, scheduler.ErrorRequestsCount);
			Assert.Equal(0, scheduler.SuccessRequestsCount);
			scheduler.IncreaseSuccessCount();
			Assert.Equal(1, scheduler.SuccessRequestsCount);

			scheduler.Poll();
			Assert.Equal(3, scheduler.LeftRequestsCount);
			Assert.Equal(1, scheduler.SuccessRequestsCount);
			Assert.Equal(1, scheduler.ErrorRequestsCount);
			Assert.Equal(4, scheduler.TotalRequestsCount);

			scheduler.Poll();
			Assert.Equal(2, scheduler.LeftRequestsCount);
			Assert.Equal(1, scheduler.SuccessRequestsCount);
			Assert.Equal(1, scheduler.ErrorRequestsCount);
			Assert.Equal(4, scheduler.TotalRequestsCount);

			scheduler.Poll();
			Assert.Equal(1, scheduler.LeftRequestsCount);
			Assert.Equal(1, scheduler.SuccessRequestsCount);
			Assert.Equal(1, scheduler.ErrorRequestsCount);
			Assert.Equal(4, scheduler.TotalRequestsCount);

			scheduler.Poll();
			Assert.Equal(0, scheduler.LeftRequestsCount);
			Assert.Equal(1, scheduler.SuccessRequestsCount);
			Assert.Equal(1, scheduler.ErrorRequestsCount);
			Assert.Equal(4, scheduler.TotalRequestsCount);

			scheduler.Poll();
			scheduler.Poll();
			Assert.Equal(0, scheduler.LeftRequestsCount);
			Assert.Equal(1, scheduler.SuccessRequestsCount);
			Assert.Equal(1, scheduler.ErrorRequestsCount);
			Assert.Equal(4, scheduler.TotalRequestsCount);

			scheduler.Clear();
		}

		[Fact]
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
			Assert.Equal(queueKey, scheduler.GetQueueKey());
			Assert.Equal(setKey, scheduler.GetSetKey());
			Assert.Equal(itemKey, scheduler.GetItemKey());
			Assert.Equal(errorCountKey, scheduler.GetErrorCountKey());
			Assert.Equal(successCountKey, scheduler.GetSuccessCountKey());

			scheduler.Dispose();
			scheduler.Dispose();
		}

		[Fact]
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
			Assert.Equal("http://www.ibm.com/4", result.Url.ToString());

			scheduler.Dispose();
		}

		[Fact]
		public void RetryRequest()
		{
			var site = new Site { EncodingName = "UTF-8", RemoveOutboundLinks = true };

			var scheduler = new QueueDuplicateRemovedScheduler();

			site.AddStartUrl("http://v.youku.com/v_show/id_XMTMyMTkzNTY1Mg==.html?spm=a2h1n.8251845.0.0");
			site.AddStartUrl("http://v.youku.com/v_show/id_XMjkzNzMwMDMyOA==.html?spm=a2h1n.8251845.0.0");
			site.AddStartUrl("http://v.youku.com/v_show/id_XMjcwNDg0NDI3Mg==.html?spm=a2h1n.8251845.0.0");
			site.AddStartUrl("http://v.youku.com/v_show/id_XMTMwNzQwMTcwMA==.html?spm=a2h1n.8251845.0.0");
			site.AddStartUrl("http://v.youku.com/v_show/id_XMjk1MzI0Mzk4NA==.html?spm=a2h1n.8251845.0.0");
			site.AddStartUrl("http://v.youku.com/v_show/id_XMjkzNzY0NzkyOA==.html?spm=a2h1n.8251845.0.0");
			site.AddStartUrl("http://www.cnblogs.com/");

			Spider spider = Spider.Create(site,
				// crawler identity
				"cnblogs_" + DateTime.Now.ToString("yyyyMMddhhmmss"),
				// use memoery queue scheduler
				scheduler,
				// default page processor will save whole html, and extract urls to target urls via regex
				new TestPageProcessor())
				// save crawler result to file in the folder: \{running directory}\data\{crawler identity}\{guid}.dsd
				.AddPipeline(new FilePipeline());

			// dowload html by http client
			spider.Downloader = new HttpClientDownloader();

			spider.ThreadNum = 1;
			// traversal deep 遍历深度
			spider.Deep = 3;

			// start crawler 启动爬虫
			spider.Run();

			Assert.Equal(5, spider.RetriedTimes.Value);
			Assert.Equal(0, scheduler.LeftRequestsCount);
			Assert.Equal(6, scheduler.SuccessRequestsCount);
			// 重试次数应该包含
			Assert.Equal(5, scheduler.ErrorRequestsCount);
		}

		class TestPageProcessor : BasePageProcessor
		{
			protected override void Handle(Page page)
			{
				if (page.Request.Url.ToString() == "http://www.cnblogs.com/")
				{
					throw new SpiderException("");
				}
				else
				{
					page.AddTargetRequest("http://www.cnblogs.com/", false);
				}
			}
		}
	}
}