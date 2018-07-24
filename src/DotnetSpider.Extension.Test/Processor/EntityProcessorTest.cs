using DotnetSpider.Common;
using DotnetSpider.Core;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using Xunit;

namespace DotnetSpider.Extension.Test.Processor
{
	[EntitySelector(Expression = "$.data[*]", Type = SelectorType.JsonPath)]
	public class Entity1
	{
		[FieldSelector(Expression = "$.age", Type = SelectorType.JsonPath)]
		public int Age { get; set; }
	}

	[TargetRequestSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
	public class Entity14
	{
		[FieldSelector(Expression = "./@data-sku")]
		public string Sku { get; set; }
	}

	[TargetRequestSelector(XPaths = new[] { "//*[@id=\"1111\"]", "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
	public class Entity16
	{
		[FieldSelector(Expression = "./@data-sku")]
		public string Sku { get; set; }
	}

	[TargetRequestSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&", @"&page=[0-1]+&" })]
	public class Entity17
	{
		[FieldSelector(Expression = "./@data-sku")]
		public string Sku { get; set; }
	}

	[TargetRequestSelector(XPaths = new[] { "//*[@id=\"1111\"]", "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-9]+&", @"&page=[0-1]+&" })]
	public class Entity18
	{
		[FieldSelector(Expression = "./@data-sku")]
		public string Sku { get; set; }
	}

	[TargetRequestSelector(XPaths = new[] { "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
	[TargetRequestSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
	public class Entity22
	{
		[FieldSelector(Expression = "./@data-sku")]
		public string Sku { get; set; }
	}

	[TargetRequestSelector(XPaths = new[] { "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-1]+&" })]
	[TargetRequestSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
	public class Entity19
	{
		[FieldSelector(Expression = "./@data-sku")]
		public string Sku { get; set; }
	}

	[TargetRequestSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-1]+&" })]
	[TargetRequestSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
	public class Entity20
	{
		[FieldSelector(Expression = "./@data-sku")]
		public string Sku { get; set; }
	}

	[TargetRequestSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
	[TargetRequestSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
	public class Entity21
	{
		[FieldSelector(Expression = "./@data-sku")]
		public string Sku { get; set; }
	}

	[TargetRequestSelector(XPaths = new string[] { null }, Patterns = new string[] { @"&page=[0-9]+&" })]
	public class Entity25
	{
		[FieldSelector(Expression = "./@data-sku")]
		public string Sku { get; set; }
	}

	public class EntityProcessorTest
	{
		[Fact(DisplayName = "TargetRequestSelector_1Region_1Pattern")]
		public void TargetRequestSelector_1Region_1Pattern()
		{
			new ModelDefinition<Entity14>();
			var processor = new EntityProcessor<Entity14>();

			Assert.Single(processor.GetTargetUrlPatterns("//*[@id=\"1111\"]"));
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());
			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"222\"]") == null);
		}

		[Fact(DisplayName = "TargetRequestSelector_2Region_1Pattern")]
		public void TargetRequestSelector_2Region_1Pattern()
		{
			new ModelDefinition<Entity16>();
			var processor = new EntityProcessor<Entity16>();

			Assert.Single(processor.GetTargetUrlPatterns("//*[@id=\"1111\"]"));
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());

			Assert.Single(processor.GetTargetUrlPatterns("//*[@id=\"2222\"]"));
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"2222\"]")[0].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[Fact(DisplayName = "TargetRequestSelector_1Region_2Pattern")]
		public void TargetRequestSelector_1Region_2Pattern()
		{
			new ModelDefinition<Entity17>();
			var processor = new EntityProcessor<Entity17>();
			Assert.Equal(2, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());
			Assert.Equal(@"&page=[0-1]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[1].ToString());
			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[Fact(DisplayName = "TargetRequestSelector_2Region_2Pattern")]
		public void TargetRequestSelector_2Region_2Pattern()
		{
			new ModelDefinition<Entity18>();
			var processor = new EntityProcessor<Entity18>();
			Assert.Equal(2, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());
			Assert.Equal(@"&page=[0-1]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[1].ToString());

			Assert.Equal(2, processor.GetTargetUrlPatterns("//*[@id=\"2222\"]").Count);
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"2222\"]")[0].ToString());
			Assert.Equal(@"&page=[0-1]+&", processor.GetTargetUrlPatterns("//*[@id=\"2222\"]")[1].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[Fact(DisplayName = "TargetRequestSelector_Multi_2Region_2Pattern")]
		public void TargetRequestSelector_Multi_2Region_2Pattern()
		{
			new ModelDefinition<Entity19>();
			var processor = new EntityProcessor<Entity19>();

			Assert.Single(processor.GetTargetUrlPatterns("//*[@id=\"1111\"]"));
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());

			Assert.Single(processor.GetTargetUrlPatterns("//*[@id=\"2222\"]"));
			Assert.Equal(@"&page=[0-1]+&", processor.GetTargetUrlPatterns("//*[@id=\"2222\"]")[0].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}
		[Fact(DisplayName = "TargetRequestSelector_Multi_2SameRegion_2Pattern")]
		public void TargetRequestSelector_Multi_2SameRegion_2Pattern()
		{
			new ModelDefinition<Entity20>();
			var processor = new EntityProcessor<Entity20>();

			Assert.Equal(2, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.Equal(@"&page=[0-1]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[1].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[Fact(DisplayName = "TargetRequestSelector_Multi_2SameRegion_2SamePattern")]
		public void TargetRequestSelector_Multi_2SameRegion_2SamePattern()
		{
			new ModelDefinition<Entity21>();
			var processor = new EntityProcessor<Entity21>();

			Assert.Single(processor.GetTargetUrlPatterns("//*[@id=\"1111\"]"));
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[Fact(DisplayName = "TargetRequestSelector_Multi_2Region_2SamePattern")]
		public void TargetRequestSelector_Multi_2Region_2SamePattern()
		{
			new ModelDefinition<Entity22>();
			var processor = new EntityProcessor<Entity22>();

			Assert.Single(processor.GetTargetUrlPatterns("//*[@id=\"1111\"]"));
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());

			Assert.Single(processor.GetTargetUrlPatterns("//*[@id=\"2222\"]"));
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"2222\"]")[0].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[Fact(DisplayName = "TargetRequestSelector_NullRegion_1Pattern")]
		public void TargetRequestSelector_NullRegion_1Pattern()
		{
			new ModelDefinition<Entity25>();
			var processor = new EntityProcessor<Entity25>();
			Assert.Single(processor.GetTargetUrlPatterns(null));
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns(null)[0].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[Fact(DisplayName = "TempEntityNoPrimaryInfo")]
		public void TempEntityNoPrimaryInfo()
		{
			EntityProcessor<Entity1> processor = new EntityProcessor<Entity1>();
			var page = new Page(new Request("http://www.abcd.com") { Site = new Site() })
			{
				Content = "{'data':[{'age':'1'},{'age':'2'}]}"
			};
			processor.Process(page, null);
			Assert.Equal(2, (page.ResultItems.GetResultItem($"DotnetSpider.Extension.Test.Processor.Entity1").Item2).Count);
		}
	}
}
