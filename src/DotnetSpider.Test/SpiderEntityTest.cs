using System.Reflection;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using Xunit;

namespace DotnetSpider.Test
{
	public class SpiderEntityTest
	{
		public class Entity1 : ISpiderEntity
		{
		}

		[Indexes(Index = new[] { "name" }, Primary = "name")]
		public class Entity2 : ISpiderEntity
		{
			[StoredAs("name", DataType.String, 10)]
			public string Name { get; set; }
		}

		[Fact]
		public void Test1()
		{
#if !NET_CORE
			var indexes = typeof(Entity1).GetCustomAttribute<Indexes>();
#else
			var indexes = typeof(Entity2).GetTypeInfo().GetCustomAttribute<Indexes>(true);
#endif

			Assert.Equal(indexes.AutoIncrement, "id");
			Assert.Equal(indexes.Primary, "id");

#if !NET_CORE
			var indexes1 = typeof(Entity2).GetCustomAttribute<Indexes>(true);
#else
			var indexes1 = typeof(Entity2).GetTypeInfo().GetCustomAttribute<Indexes>(true);
#endif

			Assert.Equal(indexes1.AutoIncrement, null);
			Assert.Equal(indexes1.Primary, "name");
		}
	}
}
