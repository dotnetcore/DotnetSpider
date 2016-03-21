using System.Threading.Tasks;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Scheduler.Component;
#if !NET_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Java2Dotnet.Spider.Test.Scheduler
{
	[TestClass]
	public class HashSetDuplicateRemoverTestcase
	{
		[TestMethod]
		public void HashSetDuplicate()
		{
			HashSetDuplicateRemover scheduler = new HashSetDuplicateRemover();
			bool isDuplicate = scheduler.IsDuplicate(new Request("http://www.a.com", 1, null), null);

			Assert.IsFalse(isDuplicate);
			isDuplicate = scheduler.IsDuplicate(new Request("http://www.a.com", 1, null), null);
			Assert.IsTrue(isDuplicate);
			isDuplicate = scheduler.IsDuplicate(new Request("http://www.b.com", 1, null), null);
			Assert.IsFalse(isDuplicate);
			isDuplicate = scheduler.IsDuplicate(new Request("http://www.b.com", 1, null), null);
			Assert.IsTrue(isDuplicate);
		}

		[TestMethod]
		public void HashSetDuplicateSynchronized()
		{
			HashSetDuplicateRemover scheduler = new HashSetDuplicateRemover();
			bool isDuplicate = scheduler.IsDuplicate(new Request("http://www.a.com", 1, null), null);

			Assert.IsFalse(isDuplicate);
			Parallel.For(0, 1000, new ParallelOptions() { MaxDegreeOfParallelism = 30 }, i =>
			{
				isDuplicate = scheduler.IsDuplicate(new Request("http://www.a.com", 1, null), null);
				Assert.IsTrue(isDuplicate);
			});
		}
	}
}
