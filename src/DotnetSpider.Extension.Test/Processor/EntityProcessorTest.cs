using DotnetSpider.Core;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using Xunit;
using DotnetSpider.Downloader;
using System;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Test.Processor
{
	[Entity(Expression = "$.data[*]", Type = SelectorType.JsonPath)]
	public class Entity1 : IBaseEntity
	{
		[Field(Expression = "$.age", Type = SelectorType.JsonPath)]
		public int Age { get; set; }
	}

	[Target(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
	public class Entity14 : IBaseEntity
	{
		[Field(Expression = "./@data-sku")]
		public string Sku { get; set; }
	}

	[Target(XPaths = new[] { "//*[@id=\"1111\"]", "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
	public class Entity16 : IBaseEntity
	{
		[Field(Expression = "./@data-sku")]
		public string Sku { get; set; }
	}

	[Target(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&", @"&page=[0-1]+&" })]
	public class Entity17 : IBaseEntity
	{
		[Field(Expression = "./@data-sku")]
		public string Sku { get; set; }
	}

	[Target(XPaths = new[] { "//*[@id=\"1111\"]", "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-9]+&", @"&page=[0-1]+&" })]
	public class Entity18 : IBaseEntity
	{
		[Field(Expression = "./@data-sku")]
		public string Sku { get; set; }
	}

	[Target(XPaths = new[] { "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
	[Target(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
	public class Entity22 : IBaseEntity
	{
		[Field(Expression = "./@data-sku")]
		public string Sku { get; set; }
	}

	[Target(XPaths = new[] { "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-1]+&" })]
	[Target(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
	public class Entity19 : IBaseEntity
	{
		[Field(Expression = "./@data-sku")]
		public string Sku { get; set; }
	}

	[Target(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-1]+&" })]
	[Target(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
	public class Entity20 : IBaseEntity
	{
		[Field(Expression = "./@data-sku")]
		public string Sku { get; set; }
	}

	[Target(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
	[Target(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
	public class Entity21 : IBaseEntity
	{
		[Field(Expression = "./@data-sku")]
		public string Sku { get; set; }
	}

	[Target(XPaths = new string[] { null }, Patterns = new string[] { @"&page=[0-9]+&" })]
	public class Entity25 : IBaseEntity
	{
		[Field(Expression = "./@data-sku")]
		public string Sku { get; set; }
	}

	public class EntityProcessorTest
	{
		[Fact(DisplayName = "TargetRequestSelector_1Region_1Pattern")]
		public void TargetRequestSelector_1Region_1Pattern()
		{
			new ModelDefinition<Entity14>();
			var processor = new EntityProcessor<Entity14>();

			Assert.True(processor.ContainsXpath("//*[@id=\"1111\"]"));
			Assert.True(processor.ContainsPattern(@"&page=[0-9]+&"));
		}

		[Fact(DisplayName = "TargetRequestSelector_2Region_1Pattern")]
		public void TargetRequestSelector_2Region_1Pattern()
		{
			new ModelDefinition<Entity16>();
			var processor = new EntityProcessor<Entity16>();

			Assert.True(processor.ContainsXpath("//*[@id=\"1111\"]"));
			Assert.True(processor.ContainsPattern(@"&page=[0-9]+&"));
			Assert.True(processor.ContainsXpath("//*[@id=\"2222\"]"));
		}

		[Fact(DisplayName = "TargetRequestSelector_1Region_2Pattern")]
		public void TargetRequestSelector_1Region_2Pattern()
		{
			new ModelDefinition<Entity17>();
			var processor = new EntityProcessor<Entity17>();

			Assert.True(processor.ContainsXpath("//*[@id=\"1111\"]"));
			Assert.True(processor.ContainsPattern(@"&page=[0-9]+&"));
			Assert.True(processor.ContainsPattern(@"&page=[0-1]+&"));
		}

		[Fact(DisplayName = "TargetRequestSelector_2Region_2Pattern")]
		public void TargetRequestSelector_2Region_2Pattern()
		{
			new ModelDefinition<Entity18>();
			var processor = new EntityProcessor<Entity18>();

			Assert.True(processor.ContainsXpath("//*[@id=\"1111\"]"));
			Assert.True(processor.ContainsXpath("//*[@id=\"2222\"]"));
			Assert.True(processor.ContainsPattern(@"&page=[0-9]+&"));
			Assert.True(processor.ContainsPattern(@"&page=[0-1]+&"));
		}

		[Fact(DisplayName = "TargetRequestSelector_Multi_2Region_2Pattern")]
		public void TargetRequestSelector_Multi_2Region_2Pattern()
		{
			new ModelDefinition<Entity19>();
			var processor = new EntityProcessor<Entity19>();

			Assert.True(processor.ContainsXpath("//*[@id=\"1111\"]"));
			Assert.True(processor.ContainsXpath("//*[@id=\"2222\"]"));
			Assert.True(processor.ContainsPattern(@"&page=[0-9]+&"));
			Assert.True(processor.ContainsPattern(@"&page=[0-1]+&"));
		}
		[Fact(DisplayName = "TargetRequestSelector_Multi_2SameRegion_2Pattern")]
		public void TargetRequestSelector_Multi_2SameRegion_2Pattern()
		{
			new ModelDefinition<Entity20>();
			var processor = new EntityProcessor<Entity20>();

			Assert.True(processor.ContainsXpath("//*[@id=\"1111\"]"));
			Assert.False(processor.ContainsXpath("//*[@id=\"2222\"]"));
			Assert.True(processor.ContainsPattern(@"&page=[0-9]+&"));
			Assert.True(processor.ContainsPattern(@"&page=[0-1]+&"));
		}

		[Fact(DisplayName = "TargetRequestSelector_Multi_2SameRegion_2SamePattern")]
		public void TargetRequestSelector_Multi_2SameRegion_2SamePattern()
		{
			new ModelDefinition<Entity21>();
			var processor = new EntityProcessor<Entity21>();

			Assert.True(processor.ContainsXpath("//*[@id=\"1111\"]"));
			Assert.True(processor.ContainsPattern(@"&page=[0-9]+&"));
		}

		[Fact(DisplayName = "TargetRequestSelector_Multi_2Region_2SamePattern")]
		public void TargetRequestSelector_Multi_2Region_2SamePattern()
		{
			new ModelDefinition<Entity22>();
			var processor = new EntityProcessor<Entity22>();

			Assert.True(processor.ContainsXpath("//*[@id=\"1111\"]"));
			Assert.True(processor.ContainsXpath("//*[@id=\"2222\"]"));
			Assert.True(processor.ContainsPattern(@"&page=[0-9]+&"));
		}

		[Fact(DisplayName = "TargetRequestSelector_NullRegion_1Pattern")]
		public void TargetRequestSelector_NullRegion_1Pattern()
		{
			new ModelDefinition<Entity25>();
			var processor = new EntityProcessor<Entity25>();
			Assert.True(processor.ContainsXpath("."));
			Assert.True(processor.ContainsPattern(@"&page=[0-9]+&"));
		}

		[Fact(DisplayName = "TempEntityNoPrimaryInfo")]
		public void TempEntityNoPrimaryInfo()
		{
			EntityProcessor<Entity1> processor = new EntityProcessor<Entity1>();
			var page = new Page(new Request("http://www.abcd.com"))
			{
				Content = "{'data':[{'age':'1'},{'age':'2'}]}"
			};
			processor.Process(page);
			Assert.Equal(2, ((List<dynamic>)page.ResultItems[$"DotnetSpider.Extension.Test.Processor.Entity1"]).Count);
		}
	}
}
