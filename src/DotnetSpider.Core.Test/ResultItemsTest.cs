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

		[Fact]
		public void AddOrUpdateOrGet()
		{
			ResultItems resultItems = new ResultItems();
			resultItems.TryAdd("a", "a");
			resultItems.TryAdd("b", "b");
			resultItems.TryAdd("c", "c");
			resultItems.TryAdd("d", 1);
			resultItems.TryAdd("e", new A { Name = "test" });
			Assert.Equal("a", resultItems.GetResultItem("a"));
			Assert.Equal("b", resultItems.GetResultItem("b"));
			Assert.Equal("c", resultItems.GetResultItem("c"));
			Assert.Equal(1, resultItems.GetResultItem("d"));
			Assert.Equal("test", resultItems.GetResultItem("e").Name);
		}

		[Fact]
		public void AddOrUpdateOrGetAsync()
		{
			ResultItems resultItems = new ResultItems();

			Parallel.For(1, 10000, new ParallelOptions
			{
				MaxDegreeOfParallelism = 10
			}, i =>
			{
				resultItems.TryAdd(i.ToString(), i);
			});

			Assert.Equal(1, resultItems.GetResultItem("1"));
		}
	}
}
