using DotnetSpider.Core;
#if !NET_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace DotnetSpider.Test
{
	[TestClass]
	public class ResultItemsTest
	{
		[TestMethod]
		public void TestOrderOfEntries()
		{
			ResultItems resultItems = new ResultItems();
			resultItems.AddOrUpdateResultItem("a", "a");
			resultItems.AddOrUpdateResultItem("b", "b");
			resultItems.AddOrUpdateResultItem("c", "c");

			resultItems.GetResultItem("a");
			resultItems.GetResultItem("b");
			resultItems.GetResultItem("c");
			//Assert.AreEqual(a, "a");
			//Assert.AreEqual(b, "b");
			//Assert.AreEqual(c, "c");
		}
	}
}
