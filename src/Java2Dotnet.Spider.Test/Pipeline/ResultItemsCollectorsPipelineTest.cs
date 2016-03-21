using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Pipeline;
#if !NET_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Java2Dotnet.Spider.Test.Pipeline
{
	[TestClass]
	public class ResultItemsCollectorsPipelineTest
	{
		readonly ResultItemsCollectorPipeline _resultItemsCollectorPipeline = new ResultItemsCollectorPipeline();

		[TestMethod]
		public void TestCollectorPipeline()
		{
			ResultItems resultItems = new ResultItems();
			resultItems.AddOrUpdateResultItem("a", "a");
			resultItems.AddOrUpdateResultItem("b", "b");
			resultItems.AddOrUpdateResultItem("c", "c");
			_resultItemsCollectorPipeline.Process(resultItems, null);
			foreach (var result in _resultItemsCollectorPipeline.GetCollected())
			{
				ResultItems items = result as ResultItems;
				 
				Assert.AreEqual(items.Results.Count, 3);
				Assert.AreEqual(items.Results["a"], "a");
				Assert.AreEqual(items.Results["b"], "b");
				Assert.AreEqual(items.Results["c"], "c");
			}
		}
	}
}
