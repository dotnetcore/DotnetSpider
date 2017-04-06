using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetSpider.Core.Test
{
	[TestClass]
	public class ResultItemsTest
	{
		public class A
		{
			public string Name { get; set; }
		}

		[TestMethod]
		public void AddOrUpdateOrGet()
		{
			ResultItems resultItems = new ResultItems();
			resultItems.AddOrUpdateResultItem("a", "a");
			resultItems.AddOrUpdateResultItem("b", "b");
			resultItems.AddOrUpdateResultItem("c", "c");
			resultItems.AddOrUpdateResultItem("d", 1);
			resultItems.AddOrUpdateResultItem("e", new A { Name = "test" });
			Assert.AreEqual("a", resultItems.GetResultItem("a"));
			Assert.AreEqual("b", resultItems.GetResultItem("b"));
			Assert.AreEqual("c", resultItems.GetResultItem("c"));
			Assert.AreEqual(1, resultItems.GetResultItem("d"));
			Assert.AreEqual("test", resultItems.GetResultItem("e").Name);
		}

		[TestMethod]
		public void AddOrUpdateOrGetAsync()
		{
			ResultItems resultItems = new ResultItems();

			Parallel.For(1, 10000, new ParallelOptions
			{
				MaxDegreeOfParallelism = 10
			}, i =>
			{
				resultItems.AddOrUpdateResultItem(i.ToString(), i);
			});

			Assert.AreEqual(1, resultItems.GetResultItem("1"));
		}
	}
}
