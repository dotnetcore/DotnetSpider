using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Processor;
using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetSpider.Extension.Test
{
	[TestClass]
	public class TargetUrlSelectorTest
	{
		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Entity14 : SpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]", "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Entity16 : SpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&", @"&page=[0-1]+&" })]
		public class Entity17 : SpiderEntity
		{
		}

		[TargetUrlsSelector()]
		public class Entity15 : SpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]", "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-9]+&", @"&page=[0-1]+&" })]
		public class Entity18 : SpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-1]+&" })]
		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Entity19 : SpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-1]+&" })]
		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Entity20 : SpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Entity21 : SpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"2222\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		[TargetUrlsSelector(XPaths = new[] { "//*[@id=\"1111\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Entity22 : SpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new[] { "" }, Patterns = new[] { "" })]
		public class Entity23 : SpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new string[] { null }, Patterns = new string[] { null })]
		public class Entity24 : SpiderEntity
		{
		}

		[TargetUrlsSelector(XPaths = new string[] { null }, Patterns = new string[] { @"&page=[0-9]+&" })]
		public class Entity25 : SpiderEntity
		{
		}

		[TestMethod]
		public void TargetUrlsSelector_1Region_1Pattern()
		{
			var entity1 = EntitySpider.GenerateEntityMetaData(typeof(Entity14).GetTypeInfo());
			var processor = new EntityProcessor(new Site(), entity1);
			Assert.AreEqual(1, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.AreEqual(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());
			Assert.IsTrue(processor.GetTargetUrlPatterns("//*[@id=\"222\"]") == null);
		}

		[TestMethod]
		public void TargetUrlsSelector_2Region_1Pattern()
		{
			var entity1 = EntitySpider.GenerateEntityMetaData(typeof(Entity16).GetTypeInfo());
			var processor = new EntityProcessor(new Site(), entity1);
			Assert.AreEqual(1, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.AreEqual(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());

			Assert.AreEqual(1, processor.GetTargetUrlPatterns("//*[@id=\"2222\"]").Count);
			Assert.AreEqual(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"2222\"]")[0].ToString());

			Assert.IsTrue(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[TestMethod]
		public void TargetUrlsSelector_1Region_2Pattern()
		{
			var entity1 = EntitySpider.GenerateEntityMetaData(typeof(Entity17).GetTypeInfo());
			var processor = new EntityProcessor(new Site(), entity1);
			Assert.AreEqual(2, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.AreEqual(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());
			Assert.AreEqual(@"&page=[0-1]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[1].ToString());
			Assert.IsTrue(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[TestMethod]
		public void TargetUrlsSelector_Null()
		{
			try
			{
				var entity2 = EntitySpider.GenerateEntityMetaData(typeof(Entity15).GetTypeInfo());
				var processor2 = new EntityProcessor(new Site(), entity2);
			}
			catch (Exception e)
			{
				Assert.AreEqual("Region xpath and patterns should not be null both.", e.Message);
				return;
			}
			throw new Exception("Failed.");
		}

		[TestMethod]
		public void TargetUrlsSelector_2Region_2Pattern()
		{
			var entity1 = EntitySpider.GenerateEntityMetaData(typeof(Entity18).GetTypeInfo());
			var processor = new EntityProcessor(new Site(), entity1);
			Assert.AreEqual(2, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.AreEqual(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());
			Assert.AreEqual(@"&page=[0-1]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[1].ToString());

			Assert.AreEqual(2, processor.GetTargetUrlPatterns("//*[@id=\"2222\"]").Count);
			Assert.AreEqual(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"2222\"]")[0].ToString());
			Assert.AreEqual(@"&page=[0-1]+&", processor.GetTargetUrlPatterns("//*[@id=\"2222\"]")[1].ToString());

			Assert.IsTrue(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[TestMethod]
		public void TargetUrlsSelector_Multi_2Region_2Pattern()
		{
			var entity1 = EntitySpider.GenerateEntityMetaData(typeof(Entity19).GetTypeInfo());
			var processor = new EntityProcessor(new Site(), entity1);
			Assert.AreEqual(1, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.AreEqual(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());

			Assert.AreEqual(1, processor.GetTargetUrlPatterns("//*[@id=\"2222\"]").Count);
			Assert.AreEqual(@"&page=[0-1]+&", processor.GetTargetUrlPatterns("//*[@id=\"2222\"]")[0].ToString());

			Assert.IsTrue(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[TestMethod]
		public void TargetUrlsSelector_Multi_2SameRegion_2Pattern()
		{
			var entity1 = EntitySpider.GenerateEntityMetaData(typeof(Entity20).GetTypeInfo());
			var processor = new EntityProcessor(new Site(), entity1);
			Assert.AreEqual(2, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.AreEqual(@"&page=[0-1]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());
			Assert.AreEqual(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[1].ToString());

			Assert.IsTrue(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[TestMethod]
		public void TargetUrlsSelector_Multi_2SameRegion_2SamePattern()
		{
			var entity1 = EntitySpider.GenerateEntityMetaData(typeof(Entity21).GetTypeInfo());
			var processor = new EntityProcessor(new Site(), entity1);
			Assert.AreEqual(1, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.AreEqual(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());

			Assert.IsTrue(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[TestMethod]
		public void TargetUrlsSelector_Multi_2Region_2SamePattern()
		{
			var entity1 = EntitySpider.GenerateEntityMetaData(typeof(Entity22).GetTypeInfo());
			var processor = new EntityProcessor(new Site(), entity1);
			Assert.AreEqual(1, processor.GetTargetUrlPatterns("//*[@id=\"1111\"]").Count);
			Assert.AreEqual(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"1111\"]")[0].ToString());

			Assert.AreEqual(1, processor.GetTargetUrlPatterns("//*[@id=\"2222\"]").Count);
			Assert.AreEqual(@"&page=[0-9]+&", processor.GetTargetUrlPatterns("//*[@id=\"2222\"]")[0].ToString());

			Assert.IsTrue(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}

		[TestMethod]
		public void TargetUrlsSelector_EmptyRegion_EmptyPattern()
		{
			try
			{
				var entity2 = EntitySpider.GenerateEntityMetaData(typeof(Entity23).GetTypeInfo());
				var processor2 = new EntityProcessor(new Site(), entity2);
			}
			catch (ArgumentNullException e)
			{
				Assert.IsNotNull(e);
				return;
			}
			throw new Exception("Failed.");
		}

		[TestMethod]
		public void TargetUrlsSelector_NullRegion_NullPattern()
		{
			try
			{
				var entity2 = EntitySpider.GenerateEntityMetaData(typeof(Entity24).GetTypeInfo());
				var processor2 = new EntityProcessor(new Site(), entity2);
			}
			catch (ArgumentNullException e)
			{
				Assert.IsNotNull(e);
				return;
			}
			throw new Exception("Failed.");
		}

		[TestMethod]
		public void TargetUrlsSelector_NullRegion_1Pattern()
		{
			var entity2 = EntitySpider.GenerateEntityMetaData(typeof(Entity25).GetTypeInfo());
			var processor = new EntityProcessor(new Site(), entity2);
			Assert.AreEqual(1, processor.GetTargetUrlPatterns(null).Count);
			Assert.AreEqual(@"&page=[0-9]+&", processor.GetTargetUrlPatterns(null)[0].ToString());

			Assert.IsTrue(processor.GetTargetUrlPatterns("//*[@id=\"3333\"]") == null);
		}
	}
}
