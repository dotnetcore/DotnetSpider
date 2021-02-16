using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using Xunit;

namespace DotnetSpider.Tests
{
	public class RequestedQueueTests
	{
		[Fact]
		public void Enqueue()
		{
			var queue = new RequestedQueue();
			var request = new Request("http://www.baidu.com") {Timeout = 2000};
			var requestHasher = new RequestHasher(new MurmurHashAlgorithmService());
			requestHasher.ComputeHash(request);
			queue.Enqueue(request);
			Assert.Equal(1, queue.Count);
			queue.Enqueue(request);
			Assert.Equal(1, queue.Count);
			var request2 = new Request("http://www.baidu.com/2") {Timeout = 2000};
			requestHasher.ComputeHash(request2);
			queue.Enqueue(request2);
			Assert.Equal(2, queue.Count);
		}

		[Fact]
		public void DequeueTimeout()
		{
			var queue = new RequestedQueue();
			var request = new Request("http://www.baidu.com") {Timeout = 2000};
			var requestHasher = new RequestHasher(new MurmurHashAlgorithmService());
			requestHasher.ComputeHash(request);
			queue.Enqueue(request);
			Assert.Equal(1, queue.Count);
			Thread.Sleep(2500);
			Assert.Null(queue.Dequeue(request.Hash));
			var timeoutRequests = queue.GetAllTimeoutList();
			Assert.Single(timeoutRequests);
			Assert.Equal(request.Hash, timeoutRequests[0].Hash);
		}

		[Fact]
		public void Dequeue()
		{
			var queue = new RequestedQueue();
			var request = new Request("http://www.baidu.com") {Timeout = 2000};
			var requestHasher = new RequestHasher(new MurmurHashAlgorithmService());
			requestHasher.ComputeHash(request);
			queue.Enqueue(request);
			Assert.Equal(1, queue.Count);
			Thread.Sleep(1000);
			var request2 = queue.Dequeue(request.Hash);
			Assert.NotNull(request2);
			Assert.Equal(request, request2);
			Assert.Equal(request.Hash, request2.Hash);
		}

		[Fact]
		public void ParallelEnqueue()
		{
			var queue = new RequestedQueue();
			var requestHasher = new RequestHasher(new MurmurHashAlgorithmService());
			Parallel.For(1, 10000, new ParallelOptions(), (i) =>
			{
				var request = new Request($"http://www.baidu.com/{i}") {Timeout = 2000};
				requestHasher.ComputeHash(request);
				queue.Enqueue(request);
			});
		}

		[Fact]
		public void ParallelDequeue()
		{
			var queue = new RequestedQueue();
			var requestHasher = new RequestHasher(new MurmurHashAlgorithmService());
			var hashes = new List<string>();
			for (var i = 0; i < 10000; ++i)
			{
				var request = new Request($"http://www.baidu.com/{i}") {Timeout = 30000};
				requestHasher.ComputeHash(request);
				hashes.Add(request.Hash);
				queue.Enqueue(request);
			}

			Parallel.ForEach(hashes, new ParallelOptions(), (hash) =>
			{
				var request = queue.Dequeue(hash);
				Assert.NotNull(request);
			});
		}
	}
}
