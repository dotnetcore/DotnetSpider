using System.Threading.Tasks;
using DotnetSpider.Core.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetSpider.Core.Test.Scheduler
{
	[TestClass]
	public class QueueSchedulerTest
	{
		[TestMethod]
		public void PushAndPollAsync()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			var spider = new DefaultSpider("test", new Site(), scheduler);
			scheduler.Init(spider);

			Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = 20 }, i =>
			{
				scheduler.Push(new Request("http://www.a.com", null));
				scheduler.Push(new Request("http://www.a.com", null));
				scheduler.Push(new Request("http://www.a.com", null));
				scheduler.Push(new Request("http://www.b.com", null));
				scheduler.Push(new Request($"http://www.{i.ToString()}.com", null));
			});
			Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = 20}, i =>
			{
				scheduler.Poll();
			});
			long left = scheduler.LeftRequestsCount;
			long total = scheduler.TotalRequestsCount;

			Assert.AreEqual(2, left);
			Assert.AreEqual(1002, total);
		}

		[TestMethod]
		public void PushAndPollDepthFirst()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			ISpider spider = new DefaultSpider("test", new Site(), scheduler);
			scheduler.Init(spider);

			scheduler.Push(new Request("http://www.a.com", null));
			scheduler.Push(new Request("http://www.a.com", null));
			scheduler.Push(new Request("http://www.a.com", null));
			scheduler.Push(new Request("http://www.b.com", null));

			var request = scheduler.Poll();
			Assert.AreEqual(request.Url.ToString(), "http://www.b.com/");

			long left = scheduler.LeftRequestsCount;
			long total = scheduler.TotalRequestsCount;

			Assert.AreEqual(left, 1);
			Assert.AreEqual(total, 2);
		}

		[TestMethod]
		public void PushAndPollBreadthFirst()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			scheduler.DepthFirst = false;
			ISpider spider = new DefaultSpider("test", new Site());
			scheduler.Init(spider);

			scheduler.Push(new Request("http://www.a.com", null));
			scheduler.Push(new Request("http://www.a.com", null));
			scheduler.Push(new Request("http://www.a.com", null));
			scheduler.Push(new Request("http://www.b.com", null));

			var request = scheduler.Poll();
			Assert.AreEqual(request.Url.ToString(), "http://www.a.com/");

			long left = scheduler.LeftRequestsCount;
			long total = scheduler.TotalRequestsCount;

			Assert.AreEqual(left, 1);
			Assert.AreEqual(total, 2);
		}

		[TestMethod]
		public void Status()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			ISpider spider = new DefaultSpider("test", new Site());
			scheduler.Init(spider);

			scheduler.Dispose();

			scheduler.Push(new Request("http://www.a.com/", null));
			scheduler.Push(new Request("http://www.b.com/", null));
			scheduler.Push(new Request("http://www.c.com/", null));
			scheduler.Push(new Request("http://www.d.com/", null));

			Assert.AreEqual(0, scheduler.ErrorRequestsCount);
			Assert.AreEqual(4, scheduler.LeftRequestsCount);
			Assert.AreEqual(4, scheduler.TotalRequestsCount);
			scheduler.IncreaseErrorCount();
			Assert.AreEqual(1, scheduler.ErrorRequestsCount);
			Assert.AreEqual(0, scheduler.SuccessRequestsCount);
			scheduler.IncreaseSuccessCount();
			Assert.AreEqual(1, scheduler.SuccessRequestsCount);

			scheduler.Poll();
			Assert.AreEqual(3, scheduler.LeftRequestsCount);
			Assert.AreEqual(1, scheduler.SuccessRequestsCount);
			Assert.AreEqual(1, scheduler.ErrorRequestsCount);
			Assert.AreEqual(4, scheduler.TotalRequestsCount);

			scheduler.Poll();
			Assert.AreEqual(2, scheduler.LeftRequestsCount);
			Assert.AreEqual(1, scheduler.SuccessRequestsCount);
			Assert.AreEqual(1, scheduler.ErrorRequestsCount);
			Assert.AreEqual(4, scheduler.TotalRequestsCount);

			scheduler.Poll();
			Assert.AreEqual(1, scheduler.LeftRequestsCount);
			Assert.AreEqual(1, scheduler.SuccessRequestsCount);
			Assert.AreEqual(1, scheduler.ErrorRequestsCount);
			Assert.AreEqual(4, scheduler.TotalRequestsCount);

			scheduler.Poll();
			Assert.AreEqual(0, scheduler.LeftRequestsCount);
			Assert.AreEqual(1, scheduler.SuccessRequestsCount);
			Assert.AreEqual(1, scheduler.ErrorRequestsCount);
			Assert.AreEqual(4, scheduler.TotalRequestsCount);

			scheduler.Poll();
			scheduler.Poll();
			Assert.AreEqual(0, scheduler.LeftRequestsCount);
			Assert.AreEqual(1, scheduler.SuccessRequestsCount);
			Assert.AreEqual(1, scheduler.ErrorRequestsCount);
			Assert.AreEqual(4, scheduler.TotalRequestsCount);

			scheduler.Dispose();
		}
	}
}
