using DotnetSpider.Common;
using System.Threading.Tasks;
using Xunit;
using DotnetSpider.Downloader;

namespace DotnetSpider.Core.Test
{

	public class HashSetDuplicateRemoverTest
	{
		[Fact(DisplayName = "HashSetDuplicateRemover_HashSetDuplicate")]
		public void HashSetDuplicate()
		{
			Scheduler.Component.HashSetDuplicateRemover scheduler = new Scheduler.Component.HashSetDuplicateRemover();

			bool isDuplicate = scheduler.IsDuplicate(new Request("http://www.a.com", null));

			Assert.False(isDuplicate);
			isDuplicate = scheduler.IsDuplicate(new Request("http://www.a.com", null));
			Assert.True(isDuplicate);
			isDuplicate = scheduler.IsDuplicate(new Request("http://www.b.com", null));
			Assert.False(isDuplicate);
			isDuplicate = scheduler.IsDuplicate(new Request("http://www.b.com", null));
			Assert.True(isDuplicate);
		}

		[Fact(DisplayName = "HashSetDuplicateSynchronized")]
		public void HashSetDuplicateSynchronized()
		{
			Scheduler.Component.HashSetDuplicateRemover scheduler = new Scheduler.Component.HashSetDuplicateRemover();
			bool isDuplicate = scheduler.IsDuplicate(new Request("http://www.a.com", null));

			Assert.False(isDuplicate);
			Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = 30 }, i =>
			{
				isDuplicate = scheduler.IsDuplicate(new Request("http://www.a.com", null));
				Assert.True(isDuplicate);
			});
		}
	}
}
