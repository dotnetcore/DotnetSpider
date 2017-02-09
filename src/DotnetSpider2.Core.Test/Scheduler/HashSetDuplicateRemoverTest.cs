using System.Threading.Tasks;
using Xunit;

namespace DotnetSpider.Core.Test.Scheduler
{
	public class HashSetDuplicateRemoverTest
	{
		[Fact]
		public void HashSetDuplicate()
		{
			Core.Scheduler.Component.HashSetDuplicateRemover scheduler = new Core.Scheduler.Component.HashSetDuplicateRemover();

			bool isDuplicate = scheduler.IsDuplicate(new Request("http://www.a.com", null));

			Assert.False(isDuplicate);
			isDuplicate = scheduler.IsDuplicate(new Request("http://www.a.com", null));
			Assert.True(isDuplicate);
			isDuplicate = scheduler.IsDuplicate(new Request("http://www.b.com", null));
			Assert.False(isDuplicate);
			isDuplicate = scheduler.IsDuplicate(new Request("http://www.b.com", null));
			Assert.True(isDuplicate);
		}

		[Fact]
		public void HashSetDuplicateSynchronized()
		{
			Core.Scheduler.Component.HashSetDuplicateRemover scheduler = new Core.Scheduler.Component.HashSetDuplicateRemover();
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
