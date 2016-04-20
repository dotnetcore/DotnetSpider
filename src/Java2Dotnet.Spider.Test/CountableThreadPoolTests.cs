using System.Threading;
using Java2Dotnet.Spider.Core;
#if !NET_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Java2Dotnet.Spider.Test
{
	[TestClass]
	public class CountableThreadPoolTests
	{
		[TestMethod]
		public void CountableThreadPoolTest()
		{
			/*构建一个threadPool*/
			var threadPool = new CountableThreadPool();
			for (int i = 0; i <= 10; i++)
			{
				threadPool.Push((obj) =>
				{
					Thread.Sleep(1000 * 30);
				}, "");
			}
			Thread.Sleep(1000 * 10);
			Assert.AreEqual(threadPool.ThreadAlive, 5);
			threadPool.WaitToExit();
			Assert.IsTrue(threadPool.ThreadAlive == 0);
		}
	}
}
