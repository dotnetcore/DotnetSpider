using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Processor;
using System;
using Xunit;

namespace DotnetSpider.Extension.Test
{

	public class TargetUrlSelectorTest
	{
		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Entity14 : SpiderEntity
		{
			[PropertyDefine(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]", "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Entity16 : SpiderEntity
		{
			[PropertyDefine(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&", @"&page=[0-1]+&" })]
		public class Entity17 : SpiderEntity
		{
			[PropertyDefine(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}

		[TargetUrlsSelector()]
		public class Entity15 : SpiderEntity
		{
			[PropertyDefine(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]", "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-9]+&", @"&page=[0-1]+&" })]
		public class Entity18 : SpiderEntity
		{
			[PropertyDefine(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-1]+&" })]
		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Entity19 : SpiderEntity
		{
			[PropertyDefine(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-1]+&" })]
		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Entity20 : SpiderEntity
		{
			[PropertyDefine(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Entity21 : SpiderEntity
		{
			[PropertyDefine(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Entity22 : SpiderEntity
		{
			[PropertyDefine(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}

		[TargetUrlsSelector(XPaths = new[] { "" }, Patterns = new[] { "" })]
		public class Entity23 : SpiderEntity
		{
			[PropertyDefine(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}

		[TargetUrlsSelector(XPaths = new string[] { null }, Patterns = new string[] { null })]
		public class Entity24 : SpiderEntity
		{
			[PropertyDefine(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}

		[TargetUrlsSelector(XPaths = new string[] { null }, Patterns = new string[] { @"&page=[0-9]+&" })]
		public class Entity25 : SpiderEntity
		{
			[PropertyDefine(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}

		[Fact]
		public void TargetUrlsSelector_1Region_1Pattern()
		{
			new EntityDefine<Entity14>();
			var processor = new EntityProcessor<Entity14>();

			Assert.Single(processor.GetTargetUrlPatterns("//*[@id=\"1111\"]"));
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());
			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"222\"]") == null);
		}

		[Fact]
		public void TargetUrlsSelector_2Region_1Pattern()
		{
			new EntityDefine<Entity16>();
			var processor = new EntityProcessor<Entity16>();

			Assert.Single(processor.GetTargetUrlPatterns("//*[@id=\"1111\"]"));
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());

			Assert.Single(processor.GetTargetUrlPatterns("//*[@id=\"2222\"]"));
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"2222\"]")[0].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[Fact]
		public void TargetUrlsSelector_1Region_2Pattern()
		{
			new EntityDefine<Entity17>();
			var processor = new EntityProcessor<Entity17>();
			Assert.Equal(2, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());
			Assert.Equal(@"&page=[0-1]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[1].ToString());
			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[Fact]
		public void TargetUrlsSelector_Null()
		{
			try
			{
				new EntityDefine<Entity15>();
				var processor = new EntityProcessor<Entity15>();
			}
			catch (Exception e)
			{
				Assert.Equal("Region xpath and patterns should not be null both.", e.Message);
				return;
			}
			throw new Exception("Failed.");
		}

		[Fact]
		public void TargetUrlsSelector_2Region_2Pattern()
		{
			new EntityDefine<Entity18>();
			var processor = new EntityProcessor<Entity18>();
			Assert.Equal(2, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());
			Assert.Equal(@"&page=[0-1]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[1].ToString());

			Assert.Equal(2, processor.GetTargetUrlPatterns("//*[@id=\"2222\"]").Count);
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"2222\"]")[0].ToString());
			Assert.Equal(@"&page=[0-1]+&", processor.GetTargetUrlPatterns("//*[@id=\"2222\"]")[1].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[Fact]
		public void TargetUrlsSelector_Multi_2Region_2Pattern()
		{
			new EntityDefine<Entity19>();
			var processor = new EntityProcessor<Entity19>();

			Assert.Single(processor.GetTargetUrlPatterns("//*[@id=\"1111\"]"));
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());

			Assert.Single(processor.GetTargetUrlPatterns("//*[@id=\"2222\"]"));
			Assert.Equal(@"&page=[0-1]+&", processor.GetTargetUrlPatterns("//*[@id=\"2222\"]")[0].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[Fact]
		public void TargetUrlsSelector_Multi_2SameRegion_2Pattern()
		{
			new EntityDefine<Entity20>();
			var processor = new EntityProcessor<Entity20>();

			Assert.Equal(2, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.Equal(@"&page=[0-1]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[1].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[Fact]
		public void TargetUrlsSelector_Multi_2SameRegion_2SamePattern()
		{
			new EntityDefine<Entity21>();
			var processor = new EntityProcessor<Entity21>();

			Assert.Single(processor.GetTargetUrlPatterns("//*[@id=\"1111\"]"));
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[Fact]
		public void TargetUrlsSelector_Multi_2Region_2SamePattern()
		{
			new EntityDefine<Entity22>();
			var processor = new EntityProcessor<Entity22>();

			Assert.Single(processor.GetTargetUrlPatterns("//*[@id=\"1111\"]"));
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());

			Assert.Single(processor.GetTargetUrlPatterns("//*[@id=\"2222\"]"));
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"2222\"]")[0].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[Fact]
		public void TargetUrlsSelector_EmptyRegion_EmptyPattern()
		{
			try
			{
				new EntityDefine<Entity23>();
				var processor = new EntityProcessor<Entity23>();
			}
			catch (ArgumentNullException e)
			{
				Assert.NotNull(e);
				return;
			}
			throw new Exception("Failed.");
		}

		[Fact]
		public void TargetUrlsSelector_NullRegion_NullPattern()
		{
			try
			{
				new EntityDefine<Entity24>();
				var processor = new EntityProcessor<Entity24>();
			}
			catch (ArgumentNullException e)
			{
				Assert.NotNull(e);
				return;
			}
			throw new Exception("Failed.");
		}

		[Fact]
		public void TargetUrlsSelector_NullRegion_1Pattern()
		{
			new EntityDefine<Entity25>();
			var processor = new EntityProcessor<Entity25>();

			Assert.Single(processor.GetTargetUrlPatterns(null));
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns(null)[0].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}
	}
}
