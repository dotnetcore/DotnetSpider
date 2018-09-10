using System.Threading.Tasks;
using DotnetSpider.Common;
using DotnetSpider.Core.Scheduler;
using Xunit;

namespace DotnetSpider.Core.Test
{

	public class QueueSchedulerTest
	{
		[Fact(DisplayName = "QueueScheduler_PushAndPollAsync")]
		public void PushAndPollAsync()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = 20 }, i =>
			{
				scheduler.Push(new Request("http://www.a.com"), null);
				scheduler.Push(new Request("http://www.a.com"), null);
				scheduler.Push(new Request("http://www.a.com"), null);
				scheduler.Push(new Request("http://www.b.com"), null);
				scheduler.Push(new Request($"http://www.{i.ToString()}.com", null), null);
			});
			Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = 20 }, i =>
			 {
				 scheduler.Poll();
			 });
			long left = scheduler.LeftRequestsCount;
			long total = scheduler.TotalRequestsCount;

			Assert.Equal(2, left);
			Assert.Equal(1002, total);
		}

		[Fact(DisplayName = "QueueScheduler_PushAndPollDepthFirst")]
		public void PushAndPollDepthFirst()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			scheduler.Push(new Request("http://www.a.com"), null);
			scheduler.Push(new Request("http://www.a.com"), null);
			scheduler.Push(new Request("http://www.a.com"), null);
			scheduler.Push(new Request("http://www.b.com"), null);

			var request = scheduler.Poll();
			Assert.Equal("http://www.b.com", request.Url.ToString());

			long left = scheduler.LeftRequestsCount;
			long total = scheduler.TotalRequestsCount;

			Assert.Equal(1, left);
			Assert.Equal(2, total);
		}

		[Fact(DisplayName = "QueueScheduler_PushAndPollBreadthFirst")]
		public void PushAndPollBreadthFirst()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			scheduler.TraverseStrategy = TraverseStrategy.Bfs;
			scheduler.Push(new Request("http://www.a.com", null), null);
			scheduler.Push(new Request("http://www.a.com", null), null);
			scheduler.Push(new Request("http://www.a.com", null), null);
			scheduler.Push(new Request("http://www.b.com", null), null);

			var request = scheduler.Poll();
			Assert.Equal("http://www.a.com", request.Url.ToString());

			long left = scheduler.LeftRequestsCount;
			long total = scheduler.TotalRequestsCount;

			Assert.Equal(1, left);
			Assert.Equal(2, total);
		}

		[Fact(DisplayName = "QueueScheduler_Status")]
		public void Status()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			scheduler.Dispose();

			scheduler.Push(new Request("http://www.a.com/", null), null);
			scheduler.Push(new Request("http://www.b.com/", null), null);
			scheduler.Push(new Request("http://www.c.com/", null), null);
			scheduler.Push(new Request("http://www.d.com/", null), null);

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
