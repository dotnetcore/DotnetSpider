using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Processor;
using System;
using System.Reflection;
using Xunit;

namespace DotnetSpider.Extension.Test
{
	public class TargetUrlSelectorTest
	{
		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Entity14 : ISpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]", "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Entity16 : ISpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&", @"&page=[0-1]+&" })]
		public class Entity17 : ISpiderEntity
		{
		}

		[TargetUrlsSelector()]
		public class Entity15 : ISpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]", "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-9]+&", @"&page=[0-1]+&" })]
		public class Entity18 : ISpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-1]+&" })]
		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Entity19 : ISpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-1]+&" })]
		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Entity20 : ISpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Entity21 : ISpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Entity22 : ISpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new[] { "" }, Patterns = new[] { "" })]
		public class Entity23 : ISpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new string[] { null }, Patterns = new string[] { null })]
		public class Entity24 : ISpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new string[] { null }, Patterns = new string[] { @"&page=[0-9]+&" })]
		public class Entity25 : ISpiderEntity
		{
		}

		[Fact]
		public void TargetUrlsSelector_1Region_1Pattern()
		{
			var entity1 = EntitySpider.ParseEntityMetaData(typeof(Entity14).GetTypeInfo());
			var processor = new EntityProcessor(new Site(), entity1);
			Assert.Equal(1, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());
			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"222\"]") == null);
		}

		[Fact]
		public void TargetUrlsSelector_2Region_1Pattern()
		{
			var entity1 = EntitySpider.ParseEntityMetaData(typeof(Entity16).GetTypeInfo());
			var processor = new EntityProcessor(new Site(), entity1);
			Assert.Equal(1, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());

			Assert.Equal(1, processor.GetTargetUrlPatterns("//*[@id=\"2222\"]").Count);
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"2222\"]")[0].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[Fact]
		public void TargetUrlsSelector_1Region_2Pattern()
		{
			var entity1 = EntitySpider.ParseEntityMetaData(typeof(Entity17).GetTypeInfo());
			var processor = new EntityProcessor(new Site(), entity1);
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
				var entity2 = EntitySpider.ParseEntityMetaData(typeof(Entity15).GetTypeInfo());
				var processor2 = new EntityProcessor(new Site(), entity2);
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
			var entity1 = EntitySpider.ParseEntityMetaData(typeof(Entity18).GetTypeInfo());
			var processor = new EntityProcessor(new Site(), entity1);
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
			var entity1 = EntitySpider.ParseEntityMetaData(typeof(Entity19).GetTypeInfo());
			var processor = new EntityProcessor(new Site(), entity1);
			Assert.Equal(1, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());

			Assert.Equal(1, processor.GetTargetUrlPatterns("//*[@id=\"2222\"]").Count);
			Assert.Equal(@"&page=[0-1]+&", processor.GetTargetUrlPatterns("//*[@id=\"2222\"]")[0].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[Fact]
		public void TargetUrlsSelector_Multi_2SameRegion_2Pattern()
		{
			var entity1 = EntitySpider.ParseEntityMetaData(typeof(Entity20).GetTypeInfo());
			var processor = new EntityProcessor(new Site(), entity1);
			Assert.Equal(2, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.Equal(@"&page=[0-1]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[1].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[Fact]
		public void TargetUrlsSelector_Multi_2SameRegion_2SamePattern()
		{
			var entity1 = EntitySpider.ParseEntityMetaData(typeof(Entity21).GetTypeInfo());
			var processor = new EntityProcessor(new Site(), entity1);
			Assert.Equal(1, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[Fact]
		public void TargetUrlsSelector_Multi_2Region_2SamePattern()
		{
			var entity1 = EntitySpider.ParseEntityMetaData(typeof(Entity22).GetTypeInfo());
			var processor = new EntityProcessor(new Site(), entity1);
			Assert.Equal(1, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());

			Assert.Equal(1, processor.GetTargetUrlPatterns("//*[@id=\"2222\"]").Count);
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"2222\"]")[0].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[Fact]
		public void TargetUrlsSelector_EmptyRegion_EmptyPattern()
		{
			try
			{
				var entity2 = EntitySpider.ParseEntityMetaData(typeof(Entity23).GetTypeInfo());
				var processor2 = new EntityProcessor(new Site(), entity2);
			}
			catch (Exception e)
			{
				Assert.Equal("值不能为 null。\r\n参数名: pattern", e.Message);
				return;
			}
			throw new Exception("Failed.");
		}

		[Fact]
		public void TargetUrlsSelector_NullRegion_NullPattern()
		{
			try
			{
				var entity2 = EntitySpider.ParseEntityMetaData(typeof(Entity24).GetTypeInfo());
				var processor2 = new EntityProcessor(new Site(), entity2);
			}
			catch (Exception e)
			{
				Assert.Equal("值不能为 null。\r\n参数名: pattern", e.Message);
				return;
			}
			throw new Exception("Failed.");
		}

		[Fact]
		public void TargetUrlsSelector_NullRegion_1Pattern()
		{
			var entity2 = EntitySpider.ParseEntityMetaData(typeof(Entity25).GetTypeInfo());
			var processor = new EntityProcessor(new Site(), entity2);
			Assert.Equal(1, processor.GetTargetUrlPatterns(".").Count);
			Assert.Equal(@"&page=[0-9]+&", processor.GetTargetUrlPatterns(".")[0].ToString());

			Assert.True(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}
	}
}
