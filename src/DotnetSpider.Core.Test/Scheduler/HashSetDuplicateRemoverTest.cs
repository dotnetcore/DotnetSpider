using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetSpider.Core.Test.Scheduler
{
	[TestClass]
	public class HashSetDuplicateRemoverTest
	{
		[TestMethod]
		public void HashSetDuplicate()
		{
			Core.Scheduler.Component.HashSetDuplicateRemover scheduler = new Core.Scheduler.Component.HashSetDuplicateRemover();

			bool isDuplicate = scheduler.IsDuplicate(new Request("http://www.a.com", null));

			Assert.IsFalse(isDuplicate);
			isDuplicate = scheduler.IsDuplicate(new Request("http://www.a.com", null));
			Assert.IsTrue(isDuplicate);
			isDuplicate = scheduler.IsDuplicate(new Request("http://www.b.com", null));
			Assert.IsFalse(isDuplicate);
			isDuplicate = scheduler.IsDuplicate(new Request("http://www.b.com", null));
			Assert.IsTrue(isDuplicate);
		}

		[TestMethod]
		public void HashSetDuplicateSynchronized()
		{
			Core.Scheduler.Component.HashSetDuplicateRemover scheduler = new Core.Scheduler.Component.HashSetDuplicateRemover();
			bool isDuplicate = scheduler.IsDuplicate(new Request("http://www.a.com", null));

			Assert.IsFalse(isDuplicate);
			Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = 30 }, i =>
			{
				isDuplicate = scheduler.IsDuplicate(new Request("http://www.a.com", null));
				Assert.IsTrue(isDuplicate);
			});
		}
	}
}
