using System;
using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Scheduler;
using Xunit;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Monitor;

namespace DotnetSpider.Extension.Test.Scheduler
{

	public class RedisSchedulerTest
	{
		private Extension.Scheduler.RedisScheduler GetRedisScheduler()
		{
			return new Extension.Scheduler.RedisScheduler("127.0.0.1:6379,serviceName=Scheduler.NET,keepAlive=8,allowAdmin=True,connectTimeout=10000,password=,abortConnect=True,connectRetry=20");
		}

		[Fact(DisplayName = "PushAndPoll1")]
		public void PushAndPoll1()
		{
			Extension.Scheduler.RedisScheduler scheduler = GetRedisScheduler();

			ISpider spider = new DefaultSpider();
			scheduler.Init(spider);
			scheduler.Dispose();

			Request request = new Request("http://www.ibm.com/developerworks/cn/java/j-javadev2-22/", null) { Site = spider.Site };
			request.PutExtra("1", "2");
			scheduler.Push(request);
			Request result = scheduler.Poll();
			Assert.Equal("http://www.ibm.com/developerworks/cn/java/j-javadev2-22/", result.Url.ToString());
			Assert.Equal("2", request.GetExtra("1"));
			Request result1 = scheduler.Poll();
			Assert.Null(result1);
			scheduler.Dispose();
		}

		[Fact(DisplayName = "RedisScheduler_PushAndPollBreadthFirst")]
		public void PushAndPollBreadthFirst()
		{
			Extension.Scheduler.RedisScheduler scheduler = GetRedisScheduler();
			scheduler.TraverseStrategy = TraverseStrategy.Bfs;
			ISpider spider = new DefaultSpider();
			scheduler.Init(spider);
			scheduler.Dispose();

			Request request1 = new Request("http://www.ibm.com/1", null) { Site = spider.Site };
			Request request2 = new Request("http://www.ibm.com/2", null) { Site = spider.Site };
			Request request3 = new Request("http://www.ibm.com/3", null) { Site = spider.Site };
			Request request4 = new Request("http://www.ibm.com/4", null) { Site = spider.Site };
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

		[Fact(DisplayName = "RedisScheduler_PushAndPollDepthFirst")]
		public void PushAndPollDepthFirst()
		{
			Extension.Scheduler.RedisScheduler scheduler = GetRedisScheduler();
			scheduler.TraverseStrategy = TraverseStrategy.Dfs;
			ISpider spider = new DefaultSpider();
			scheduler.Init(spider);
			scheduler.Dispose();
			Request request1 = new Request("http://www.ibm.com/1", null) { Site = spider.Site };
			Request request2 = new Request("http://www.ibm.com/2", null) { Site = spider.Site };
			Request request3 = new Request("http://www.ibm.com/3", null) { Site = spider.Site };
			Request request4 = new Request("http://www.ibm.com/4", null) { Site = spider.Site };
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

		[Fact(DisplayName = "LoadPerformace")]
		public void LoadPerformace()
		{
			Extension.Scheduler.RedisScheduler scheduler = GetRedisScheduler();
			Spider spider = new DefaultSpider("test", new Site());
			spider.Monitor = new LogMonitor();
			scheduler.Init(spider);
			scheduler.Dispose();
			var start = DateTime.Now;
			for (int i = 0; i < 40000; i++)
			{
				scheduler.Push(new Request("http://www.a.com/" + i, null) { Site = spider.Site });
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

		[Fact(DisplayName = "RedisScheduler_Load")]
		public void Load()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			ISpider spider = new DefaultSpider("test", new Site());
			scheduler.Init(spider);

			scheduler.Push(new Request("http://www.a.com/", null) { Site = spider.Site });
			scheduler.Push(new Request("http://www.b.com/", null) { Site = spider.Site });
			scheduler.Push(new Request("http://www.c.com/", null) { Site = spider.Site });
			scheduler.Push(new Request("http://www.d.com/", null) { Site = spider.Site });

			Extension.Scheduler.RedisScheduler redisScheduler = GetRedisScheduler();
			redisScheduler.Init(spider);

			redisScheduler.Dispose();

			redisScheduler.Import(scheduler.All);

			Assert.Equal("http://www.d.com/", redisScheduler.Poll().Url.ToString());
			Assert.Equal("http://www.c.com/", redisScheduler.Poll().Url.ToString());
			Assert.Equal("http://www.b.com/", redisScheduler.Poll().Url.ToString());
			Assert.Equal("http://www.a.com/", redisScheduler.Poll().Url.ToString());

			redisScheduler.Dispose();
		}

		[Fact(DisplayName = "RedisScheduler_Status")]
		public void Status()
		{
			Extension.Scheduler.RedisScheduler scheduler = GetRedisScheduler();
			ISpider spider = new DefaultSpider("test", new Site());
			scheduler.Init(spider);

			scheduler.Dispose();

			scheduler.Push(new Request("http://www.a.com/", null) { Site = spider.Site });
			scheduler.Push(new Request("http://www.b.com/", null) { Site = spider.Site });
			scheduler.Push(new Request("http://www.c.com/", null) { Site = spider.Site });
			scheduler.Push(new Request("http://www.d.com/", null) { Site = spider.Site });

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

			scheduler.Dispose();
		}

		//[Fact]
		//public void MultiInit()
		//{
		//	Extension.Scheduler.RedisScheduler scheduler = GetRedisScheduler();

		//	ISpider spider = new DefaultSpider();
		//	scheduler.Init(spider);
		//	string queueKey = scheduler.GetQueueKey();
		//	string setKey = scheduler.GetSetKey();
		//	string itemKey = scheduler.GetItemKey();
		//	string errorCountKey = scheduler.GetErrorCountKey();
		//	string successCountKey = scheduler.GetSuccessCountKey();
		//	scheduler.Init(spider);
		//	Assert.Equal(queueKey, scheduler.GetQueueKey());
		//	Assert.Equal(setKey, scheduler.GetSetKey());
		//	Assert.Equal(itemKey, scheduler.GetItemKey());
		//	Assert.Equal(errorCountKey, scheduler.GetErrorCountKey());
		//	Assert.Equal(successCountKey, scheduler.GetSuccessCountKey());

		//	scheduler.Dispose();
		//	scheduler.Dispose();
		//}

		[Fact(DisplayName = "RedisScheduler_Clear")]
		public void Clear()
		{
			Extension.Scheduler.RedisScheduler scheduler = GetRedisScheduler();

			ISpider spider = new DefaultSpider();
			scheduler.Init(spider);
			scheduler.Dispose();
			Request request1 = new Request("http://www.ibm.com/1", null) { Site = spider.Site };
			Request request2 = new Request("http://www.ibm.com/2", null) { Site = spider.Site };
			Request request3 = new Request("http://www.ibm.com/3", null) { Site = spider.Site };
			Request request4 = new Request("http://www.ibm.com/4", null) { Site = spider.Site };
			scheduler.Push(request1);
			scheduler.Push(request2);
			scheduler.Push(request3);
			scheduler.Push(request4);

			Request result = scheduler.Poll();
			Assert.Equal("http://www.ibm.com/4", result.Url.ToString());

			scheduler.Dispose();
		}

		[Fact(DisplayName = "RedisScheduler_RetryRequest")]
		public void RetryRequest()
		{
			var site = new Site { EncodingName = "UTF-8" };

			var scheduler = new QueueDuplicateRemovedScheduler();

			site.AddStartUrl("http://www.baidu.com");
			site.AddStartUrl("http://www.163.com/");

			Spider spider = Spider.Create(site,
				// crawler identity
				"cnblogs_" + DateTime.Now.ToString("yyyyMMddhhmmss"),
				// use memoery queue scheduler
				scheduler,
				// default page processor will save whole html, and extract urls to target urls via regex
				new TestPageProcessor())
				// save crawler result to file in the folder: \{running directory}\data\{crawler identity}\{guid}.dsd
				.AddPipeline(new FilePipeline());
			spider.Monitor = new LogMonitor();
			// dowload html by http client
			spider.Downloader = new HttpClientDownloader();

			spider.ThreadNum = 1;
			// traversal deep 遍历深度
			spider.Scheduler.Depth = 3;
			spider.ClearSchedulerAfterCompleted = false;
			spider.EmptySleepTime = 6000;
			// start crawler 启动爬虫
			spider.Run();

			Assert.Equal(5, spider.RetriedTimes.Value);
			Assert.Equal(0, scheduler.LeftRequestsCount);
			Assert.Equal(1, scheduler.SuccessRequestsCount);
			// 重试次数应该包含
			Assert.Equal(5, scheduler.ErrorRequestsCount);
		}

		class TestPageProcessor : BasePageProcessor
		{
			protected override void Handle(Page page)
			{
				if (page.Request.Url.ToString() == "http://www.163.com/")
				{
					throw new SpiderException("");
				}
				else
				{
					page.AddTargetRequest("http://www.163.com/", 0, false);
				}
			}
		}
	}
}