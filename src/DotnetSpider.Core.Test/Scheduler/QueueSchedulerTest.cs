using System.Threading.Tasks;
using DotnetSpider.Core.Scheduler;
using Xunit;

namespace DotnetSpider.Core.Test.Scheduler
{

	public class QueueSchedulerTest
	{
		[Fact]
		public void PushAndPollAsync()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			var spider = new DefaultSpider("test", new Site(), scheduler);
			scheduler.Init(spider);

			Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = 20 }, i =>
			{
				scheduler.Push(new Request("http://www.a.com", null) { Site = spider.Site });
				scheduler.Push(new Request("http://www.a.com", null) { Site = spider.Site });
				scheduler.Push(new Request("http://www.a.com", null) { Site = spider.Site });
				scheduler.Push(new Request("http://www.b.com", null) { Site = spider.Site });
				scheduler.Push(new Request($"http://www.{i.ToString()}.com", null) { Site = spider.Site });
			});
			Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = 20}, i =>
			{
				scheduler.Poll();
			});
			long left = scheduler.LeftRequestsCount;
			long total = scheduler.TotalRequestsCount;

			Assert.Equal(2, left);
			Assert.Equal(1002, total);
		}

		[Fact]
		public void PushAndPollDepthFirst()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			ISpider spider = new DefaultSpider("test", new Site(), scheduler);
			scheduler.Init(spider);

			scheduler.Push(new Request("http://www.a.com", null) { Site = spider.Site });
			scheduler.Push(new Request("http://www.a.com", null) { Site = spider.Site });
			scheduler.Push(new Request("http://www.a.com", null) { Site = spider.Site });
			scheduler.Push(new Request("http://www.b.com", null) { Site = spider.Site });

			var request = scheduler.Poll();
			Assert.Equal("http://www.b.com/", request.Url.ToString());

			long left = scheduler.LeftRequestsCount;
			long total = scheduler.TotalRequestsCount;

			Assert.Equal(1, left);
			Assert.Equal(2, total);
		}

		[Fact]
		public void PushAndPollBreadthFirst()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			scheduler.TraverseStrategy =  TraverseStrategy.BFS;
			ISpider spider = new DefaultSpider("test", new Site());
			scheduler.Init(spider);

			scheduler.Push(new Request("http://www.a.com", null) { Site = spider.Site });
			scheduler.Push(new Request("http://www.a.com", null) { Site = spider.Site });
			scheduler.Push(new Request("http://www.a.com", null) { Site = spider.Site });
			scheduler.Push(new Request("http://www.b.com", null) { Site = spider.Site });

			var request = scheduler.Poll();
			Assert.Equal("http://www.a.com/", request.Url.ToString());

			long left = scheduler.LeftRequestsCount;
			long total = scheduler.TotalRequestsCount;

			Assert.Equal(1, left);
			Assert.Equal(2, total);
		}

		[Fact]
		public void Status()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
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
	}
}
