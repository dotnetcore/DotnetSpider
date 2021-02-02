using System;
using System.Threading.Tasks;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using DotnetSpider.Scheduler.Component;
using Xunit;

namespace DotnetSpider.Tests
{
	public class HashSetDuplicateRemoverTests
	{
		[Fact(DisplayName = "HashSetDuplicate")]
		public async Task HashSetDuplicate()
		{
			var hashAlgorithm = new MurmurHashAlgorithmService();
			var scheduler = new HashSetDuplicateRemover();
			var requestHasher = new RequestHasher(hashAlgorithm);

			var ownerId = Guid.NewGuid().ToString("N");
			var r1 = new Request("http://www.a.com") {Owner = ownerId};
			r1.Headers.Accept = ("asdfasdfasdf");
			r1.Hash = requestHasher.ComputeHash(r1);
			var isDuplicate = await scheduler.IsDuplicateAsync(r1);

			Assert.False(isDuplicate);
			var r2 = new Request("http://www.a.com") {Owner = ownerId};
			r2.Hash = requestHasher.ComputeHash(r2);
			isDuplicate = await scheduler.IsDuplicateAsync(r2);
			Assert.True(isDuplicate);
			var r3 = new Request("http://www.b.com") {Owner = ownerId};
			r3.Hash = requestHasher.ComputeHash(r3);
			isDuplicate = await scheduler.IsDuplicateAsync(r3);
			Assert.False(isDuplicate);
			var r4 = new Request("http://www.b.com") {Owner = ownerId};
			r4.Hash = requestHasher.ComputeHash(r4);

			isDuplicate = await scheduler.IsDuplicateAsync(r4);
			Assert.True(isDuplicate);
		}

		[Fact(DisplayName = "ParallelHashSetDuplicate")]
		public async Task ParallelHashSetDuplicate()
		{
			var hashAlgorithm = new MurmurHashAlgorithmService();
			var ownerId = Guid.NewGuid().ToString("N");
			var scheduler = new HashSetDuplicateRemover();
			var requestHasher = new RequestHasher(hashAlgorithm);
			var r1 = new Request("http://www.a.com") {Owner = ownerId};
			r1.Hash = requestHasher.ComputeHash(r1);
			var isDuplicate = await scheduler.IsDuplicateAsync(r1);

			Assert.False(isDuplicate);
			Parallel.For(0, 1000, new ParallelOptions {MaxDegreeOfParallelism = 30}, async i =>
			{
				var r = new Request("http://www.a.com") {Owner = ownerId};
				r.Hash = requestHasher.ComputeHash(r);
				isDuplicate = await scheduler.IsDuplicateAsync(r);
				Assert.True(isDuplicate);
			});
		}
	}
}
