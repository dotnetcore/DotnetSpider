using DotnetSpider.Core;
using  Xunit;

namespace DotnetSpider.Test
{
	
	public class ResultItemsTest
	{
		[Fact]
		public void TestOrderOfEntries()
		{
			ResultItems resultItems = new ResultItems();
			resultItems.AddOrUpdateResultItem("a", "a");
			resultItems.AddOrUpdateResultItem("b", "b");
			resultItems.AddOrUpdateResultItem("c", "c");

			resultItems.GetResultItem("a");
			resultItems.GetResultItem("b");
			resultItems.GetResultItem("c");
			//Assert.Equal(a, "a");
			//Assert.Equal(b, "b");
			//Assert.Equal(c, "c");
		}
	}
}
