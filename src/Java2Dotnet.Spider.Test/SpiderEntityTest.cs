using System.Reflection;
using Java2Dotnet.Spider.Extension.Model;
using Java2Dotnet.Spider.Extension.Model.Attribute;
using Java2Dotnet.Spider.Extension.ORM;
#if !NET_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Java2Dotnet.Spider.Test
{
	[TestClass]
	public class SpiderEntityTest
	{
		public class Entity1 : BaseEntity
		{
		}

		[Indexes(Index = new[] { "name" }, Primary = "name")]
		public class Entity2 : BaseEntity
		{
			[StoredAs("name", StoredAs.ValueType.String)]
			public string Name { get; set; }
		}

		[TestMethod]
		public void Test1()
		{
#if !NET_CORE
			var indexes = typeof(Entity1).GetCustomAttribute<Indexes>();
#else
			var indexes = typeof(Entity2).GetTypeInfo().GetCustomAttribute<Indexes>(true);
#endif

			Assert.AreEqual(indexes.AutoIncrement, "id");
			Assert.AreEqual(indexes.Primary, "id");

#if !NET_CORE
			var indexes1 = typeof(Entity2).GetCustomAttribute<Indexes>(true);
#else
			var indexes1 = typeof(Entity2).GetTypeInfo().GetCustomAttribute<Indexes>(true);
#endif


			Assert.AreEqual(indexes1.AutoIncrement, null);
			Assert.AreEqual(indexes1.Primary, "name");
		}
	}
}
