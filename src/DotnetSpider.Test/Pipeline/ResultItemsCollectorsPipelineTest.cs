//using DotnetSpider.Core;
//using DotnetSpider.Core.Pipeline;

//namespace DotnetSpider.Test.Pipeline
//{
//	
//	public class ResultItemsCollectorsPipelineTest
//	{
//		readonly ResultItemsCollectorPipeline _resultItemsCollectorPipeline = new ResultItemsCollectorPipeline();

//		[Fact]
//		public void TestCollectorPipeline()
//		{
//			ResultItems resultItems = new ResultItems();
//			resultItems.AddOrUpdateResultItem("a", "a");
//			resultItems.AddOrUpdateResultItem("b", "b");
//			resultItems.AddOrUpdateResultItem("c", "c");
//			_resultItemsCollectorPipeline.Process(resultItems, null);
//			foreach (var result in _resultItemsCollectorPipeline.GetCollected())
//			{
//				ResultItems items = result as ResultItems;
				 
//				Assert.Equal(items.Results.Count, 3);
//				Assert.Equal(items.Results["a"], "a");
//				Assert.Equal(items.Results["b"], "b");
//				Assert.Equal(items.Results["c"], "c");
//			}
//		}
//	}
//}
