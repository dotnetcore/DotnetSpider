using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using DotnetSpider.MySql.Scheduler;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace DotnetSpider.Tests
{
	public class MySqlQueueSchedulerTests
	{
		class Opt : IOptions<MySqlSchedulerOptions>
		{
			public MySqlSchedulerOptions Value { get; }

			public Opt()
			{
				Value = new MySqlSchedulerOptions
				{
#if DEBUG
					ConnectionString =
						"Database='test';Data Source=localhost;password=1qazZAQ!;User ID=root;Port=3306;SslMode=None;Allow User Variables=True;AllowPublicKeyRetrieval=True"
#else
					ConnectionString =
						"Database='mysql';Data Source=localhost;password=1qazZAQ!;User ID=root;Port=3306;SslMode=None;Allow User Variables=True;AllowPublicKeyRetrieval=True"
#endif
				};
			}
		}

		private readonly ITestOutputHelper _testOutputHelper;

		public MySqlQueueSchedulerTests(ITestOutputHelper testOutputHelper)
		{
			_testOutputHelper = testOutputHelper;
		}

		private static readonly RequestHasher _hashAlgorithm = new(new MurmurHashAlgorithmService());

		[Fact]
		public async Task ParallelEnqueueAndDequeueQueueBfs()
		{
			var ownerId = Guid.NewGuid().ToString("N");
			var scheduler = new MySqlQueueBfsScheduler(_hashAlgorithm, new Opt());
			await scheduler.InitializeAsync(ownerId);

			ParallelUtilities.For(0, 1000, new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = 20}, async i =>
			{
				await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
				await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
				await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
				await scheduler.EnqueueAsync(new[] {new Request("http://www.b.com") {Owner = ownerId}});
				var cnt = await scheduler.EnqueueAsync(
					new[] {new Request($"http://www.{i.ToString()}.com") {Owner = ownerId}});

				_testOutputHelper.WriteLine($"Enqueue {i}: {cnt}");
			});

			_testOutputHelper.WriteLine($"End");
			ParallelUtilities.For(0, 1000, new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = 20},
				async _ => { await scheduler.DequeueAsync(); });

			Assert.Equal(1002, scheduler.GetTotalAsync().Result);
			await scheduler.CleanAsync();
		}

		[Fact]
		public async Task EnqueueAndDequeueQueueBfs()
		{
			var ownerId = Guid.NewGuid().ToString("N");
			var scheduler = new MySqlQueueBfsScheduler(_hashAlgorithm, new Opt());
			await scheduler.InitializeAsync(ownerId);
			await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
			await scheduler.EnqueueAsync(new[] {new Request("http://www.b.com") {Owner = ownerId}});
			await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
			await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});

			var request = (await scheduler.DequeueAsync()).First();
			Assert.Equal("http://www.a.com/", request.RequestUri.ToString());
			Assert.Equal(2, scheduler.GetTotalAsync().Result);
			await scheduler.CleanAsync();
		}

		[Fact]
		public async Task EnqueueAndDequeueQueueDfs()
		{
			var ownerId = Guid.NewGuid().ToString("N");
			var scheduler = new MySqlQueueDfsScheduler(_hashAlgorithm, new Opt());
			await scheduler.InitializeAsync(ownerId);

			await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
			await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
			await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
			await scheduler.EnqueueAsync(new[] {new Request("http://www.b.com") {Owner = ownerId}});

			var request = (await scheduler.DequeueAsync()).First();
			Assert.Equal("http://www.b.com/", request.RequestUri.ToString());
			Assert.Equal(2, await scheduler.GetTotalAsync());
		}

		[Fact]
		public async Task ParallelEnqueueAndDequeueQueueDfs()
		{
			var ownerId = Guid.NewGuid().ToString("N");
			var scheduler = new MySqlQueueDfsScheduler(_hashAlgorithm, new Opt());
			await scheduler.InitializeAsync(ownerId);

			ParallelUtilities.For(0, 1000, new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = 10}, async i =>
			{
				await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
				await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
				await scheduler.EnqueueAsync(new[] {new Request("http://www.a.com") {Owner = ownerId}});
				await scheduler.EnqueueAsync(new[] {new Request("http://www.b.com") {Owner = ownerId}});
				await scheduler.EnqueueAsync(new[] {new Request($"http://www.{i.ToString()}.com") {Owner = ownerId}});
			});
			ParallelUtilities.For(0, 1000, new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = 20},
				async _ => { await scheduler.DequeueAsync(); });

			Assert.Equal(1002, await scheduler.GetTotalAsync());
		}
	}
}
