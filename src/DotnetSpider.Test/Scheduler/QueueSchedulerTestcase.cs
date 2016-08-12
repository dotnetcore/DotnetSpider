using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Core.Scheduler;
using Xunit;

namespace DotnetSpider.Test.Scheduler
{
	
	public class QueueSchedulerTestcase
	{
		[Fact]
		public void QueueSchedulerPushPollSynchronized()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			var spider = new DefaultSpider("test", new Site());

			Parallel.For(0, 1000, new ParallelOptions() { MaxDegreeOfParallelism = 30 }, i =>
			{
				scheduler.Push(new Request("http://www.a.com", 1, null));
				scheduler.Push(new Request("http://www.a.com", 1, null));
				scheduler.Push(new Request("http://www.a.com", 1, null));

				scheduler.Push(new Request("http://www.b.com", 1, null));

				scheduler.Push(new Request($"http://www.{i.ToString()}.com", 1, null));
			});

			Parallel.For(0, 1000, new ParallelOptions() { MaxDegreeOfParallelism = 30 }, i =>
			{
				scheduler.Poll();
			});

			long left = scheduler.GetLeftRequestsCount();
			long total = scheduler.GetTotalRequestsCount();

			Assert.Equal(left, 2);
			Assert.Equal(total, 1002);
		}

		[Fact]
		public void QueueSchedulerPush()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			ISpider spider = new DefaultSpider("test", new Site());
			scheduler.Push(new Request("http://www.a.com", 1, null));
			scheduler.Push(new Request("http://www.a.com", 1, null));
			scheduler.Push(new Request("http://www.a.com", 1, null));

			scheduler.Push(new Request("http://www.b.com", 1, null));
			long left = scheduler.GetLeftRequestsCount();
			long total = scheduler.GetTotalRequestsCount();

			Assert.Equal(left, 2);
			Assert.Equal(total, 2);
		}


		[Fact]
		public void QueueSchedulerPoll()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			ISpider spider = new DefaultSpider("test", new Site());
			scheduler.Push(new Request("http://www.a.com", 1, null));
			scheduler.Push(new Request("http://www.a.com", 1, null));
			scheduler.Push(new Request("http://www.a.com", 1, null));

			scheduler.Push(new Request("http://www.b.com", 1, null));

			var request = scheduler.Poll();
			Assert.Equal(request.Url.ToString(), "http://www.a.com");

			long left = scheduler.GetLeftRequestsCount();
			long total = scheduler.GetTotalRequestsCount();

			Assert.Equal(left, 1);
			Assert.Equal(total, 2);
		}
	}
}
