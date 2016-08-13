using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Core.Scheduler;
using Xunit;

namespace DotnetSpider.Test.Scheduler
{
	public class QueueScheduler
	{
		[Fact]
		public void PushAndPollAsync()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			var spider = new DefaultSpider("test", new Core.Site());
			scheduler.Init(spider);

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
		public void PushAndPoll()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			ISpider spider = new DefaultSpider("test", new Core.Site());
			scheduler.Init(spider);

			scheduler.Push(new Request("http://www.a.com", 1, null));
			scheduler.Push(new Request("http://www.a.com", 1, null));
			scheduler.Push(new Request("http://www.a.com", 1, null));
			scheduler.Push(new Request("http://www.b.com", 1, null));

			var request = scheduler.Poll();
			Assert.Equal(request.Url.ToString(), "http://www.a.com/");

			long left = scheduler.GetLeftRequestsCount();
			long total = scheduler.GetTotalRequestsCount();

			Assert.Equal(left, 1);
			Assert.Equal(total, 2);
		}

		[Fact]
		public void Status()
		{
			QueueDuplicateRemovedScheduler scheduler = new QueueDuplicateRemovedScheduler();
			ISpider spider = new DefaultSpider("test", new Core.Site());
			scheduler.Init(spider);

			scheduler.Clear();

			scheduler.Push(new Request("http://www.a.com/", 1, null));
			scheduler.Push(new Request("http://www.b.com/", 1, null));
			scheduler.Push(new Request("http://www.c.com/", 1, null));
			scheduler.Push(new Request("http://www.d.com/", 1, null));

			Assert.Equal(0, scheduler.GetErrorRequestsCount());
			Assert.Equal(4, scheduler.GetLeftRequestsCount());
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
	}
}
