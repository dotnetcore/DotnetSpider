using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using DotnetSpider.Scheduler;
using DotnetSpider.Scheduler.Component;
using Xunit;

namespace DotnetSpider.Tests
{
	public class QueueSchedulerTests
	{
		private static readonly RequestHasher _hashAlgorithm = new(new MurmurHashAlgorithmService());

		[Fact(DisplayName = "ParallelEnqueueAndDequeueQueueBfs")]
		public async Task ParallelEnqueueAndDequeueQueueBfs()
		{
			var scheduler = new QueueDistinctBfsScheduler(new HashSetDuplicateRemover(), _hashAlgorithm);
			var ownerId = Guid.NewGuid().ToString("N");
			await scheduler.InitializeAsync(ownerId);
			ParallelUtilities.For(0, 1000, new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = 20}, async i =>
			{
				await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
				await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
				await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
				await scheduler.EnqueueAsync(new[] {new Request("http://www.b.com") {Owner = ownerId}});
				await scheduler.EnqueueAsync(new[] {new Request($"http://www.{i.ToString()}.com") {Owner = ownerId}});
			});
			Parallel.For(0, 1000, new ParallelOptions {MaxDegreeOfParallelism = 20},
				async i => { await scheduler.DequeueAsync(); });

			Assert.Equal(1002, scheduler.GetTotalAsync().Result);
		}

		[Fact(DisplayName = "EnqueueAndDequeueQueueBfs")]
		public async Task EnqueueAndDequeueQueueBfs()
		{
			var ownerId = Guid.NewGuid().ToString("N");
			var scheduler = new QueueDistinctBfsScheduler(new HashSetDuplicateRemover(), _hashAlgorithm);
			await scheduler.InitializeAsync(ownerId);
			await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
			await scheduler.EnqueueAsync(new[] {new Request("http://www.b.com") {Owner = ownerId}});
			await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
			await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});

			var request = (await scheduler.DequeueAsync()).First();
			Assert.Equal("http://www.a.com/", request.RequestUri.ToString());
			Assert.Equal(2, scheduler.GetTotalAsync().Result);
		}

		[Fact(DisplayName = "EnqueueAndDequeueQueueDfs")]
		public async Task EnqueueAndDequeueQueueDfs()
		{
			var ownerId = Guid.NewGuid().ToString("N");
			var scheduler = new QueueDistinctDfsScheduler(new HashSetDuplicateRemover(), _hashAlgorithm);
			await scheduler.InitializeAsync(ownerId);
			await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
			await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
			await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
			await scheduler.EnqueueAsync(new[] {new Request("http://www.b.com") {Owner = ownerId}});

			var request = (await scheduler.DequeueAsync()).First();
			Assert.Equal("http://www.b.com/", request.RequestUri.ToString());
			Assert.Equal(2, scheduler.GetTotalAsync().Result);
		}

		[Fact(DisplayName = "ParallelEnqueueAndDequeueQueueDfs")]
		public async Task ParallelEnqueueAndDequeueQueueDfs()
		{
			var ownerId = Guid.NewGuid().ToString("N");
			var scheduler = new QueueDistinctDfsScheduler(new HashSetDuplicateRemover(), _hashAlgorithm);
			await scheduler.InitializeAsync(ownerId);

			ParallelUtilities.For(0, 1000, new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = 10}, async i =>
			{
				await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
				await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
				await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
				await scheduler.EnqueueAsync(new[] {new Request("http://www.b.com") {Owner = ownerId}});
				await scheduler.EnqueueAsync(new[] {new Request($"http://www.{i.ToString()}.com") {Owner = ownerId}});
			});
			Parallel.For(0, 1000, new ParallelOptions {MaxDegreeOfParallelism = 20},
				async i => { await scheduler.DequeueAsync(); });

			Assert.Equal(1002, scheduler.GetTotalAsync().Result);
		}
	}
}
