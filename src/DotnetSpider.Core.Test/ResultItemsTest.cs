using System.Threading.Tasks;
using Xunit;

namespace DotnetSpider.Core.Test
{
	public class ResultItemsTest
	{
		public class A
		{
			public string Name { get; set; }
		}

		[Fact(DisplayName = "ResultItems_AddOrUpdateOrGet")]
		public void AddOrUpdateOrGet()
		{
			ResultItems resultItems = new ResultItems();
			resultItems.AddOrUpdateResultItem("a", "a");
			resultItems.AddOrUpdateResultItem("b", "b");
			resultItems.AddOrUpdateResultItem("c", "c");
			resultItems.AddOrUpdateResultItem("d", 1);
			resultItems.AddOrUpdateResultItem("e", new A { Name = "test" });
			Assert.Equal("a", resultItems.GetResultItem("a"));
			Assert.Equal("b", resultItems.GetResultItem("b"));
			Assert.Equal("c", resultItems.GetResultItem("c"));
			Assert.Equal(1, resultItems.GetResultItem("d"));
			Assert.Equal("test", resultItems.GetResultItem("e").Name);
		}

		[Fact(DisplayName = "ResultItems_AddOrUpdateOrGetAsync")]
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

			Assert.Equal(1, resultItems.GetResultItem("1"));
		}
	}
}
