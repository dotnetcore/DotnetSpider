using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Core.Scheduler.Component;
using Xunit;

namespace DotnetSpider.Test.Scheduler
{
	public class HashSetDuplicateRemoverTestcase
	{
		[Fact]
		public void HashSetDuplicate()
		{
			HashSetDuplicateRemover scheduler = new HashSetDuplicateRemover();

			bool isDuplicate = scheduler.IsDuplicate(new Request("http://www.a.com", 1, null));

			Assert.False(isDuplicate);
			isDuplicate = scheduler.IsDuplicate(new Request("http://www.a.com", 1, null));
			Assert.True(isDuplicate);
			isDuplicate = scheduler.IsDuplicate(new Request("http://www.b.com", 1, null));
			Assert.False(isDuplicate);
			isDuplicate = scheduler.IsDuplicate(new Request("http://www.b.com", 1, null));
			Assert.True(isDuplicate);
		}

		[Fact]
		public void HashSetDuplicateSynchronized()
		{
			HashSetDuplicateRemover scheduler = new HashSetDuplicateRemover();
			bool isDuplicate = scheduler.IsDuplicate(new Request("http://www.a.com", 1, null));

			Assert.False(isDuplicate);
			Parallel.For(0, 1000, new ParallelOptions() { MaxDegreeOfParallelism = 30 }, i =>
			{
				isDuplicate = scheduler.IsDuplicate(new Request("http://www.a.com", 1, null));
				Assert.True(isDuplicate);
			});
		}
	}
}
