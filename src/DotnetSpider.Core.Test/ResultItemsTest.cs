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
			resultItems["a"] = "a";
			resultItems["b"] = "b";
			resultItems["c"] = "c";
			resultItems["d"] = 1;
			resultItems["e"] = new A { Name = "test" };

			Assert.Equal("a", resultItems["a"]);
			Assert.Equal("b", resultItems["b"]);
			Assert.Equal("c", resultItems["c"]);
			Assert.Equal(1, resultItems["d"]);
			Assert.Equal("test", ((A)resultItems["e"]).Name);
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
				resultItems[i.ToString()] = i;
			});

			Assert.Equal(1, resultItems["1"]);
		}
	}
}
