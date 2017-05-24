using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Extension.Test
{
	[TestClass]
	public class EntityExtractorTest
	{
		[TestMethod]
		public void Extract()
		{
			var entityMetadata = EntitySpider.GenerateEntityMetaData(typeof(Product).GetTypeInfo());
			EntityExtractor extractor = new EntityExtractor("test", null, entityMetadata);
			var results = extractor.Extract(new Page(new Request("http://list.jd.com/list.html?cat=9987,653,655&page=2&JL=6_0_0&ms=5#J_main", new Dictionary<string, dynamic>
			{
				{ "cat", "手机" },
				{ "cat3", "110" }
			}), ContentType.Html, null)
			{
				Content = File.ReadAllText(Path.Combine(SpiderConsts.BaseDirectory, "Jd.html"))
			});
			Assert.AreEqual(60, results.Count);
			Assert.AreEqual("手机", results[0].GetValue("CategoryName"));
			Assert.AreEqual("110", results[0].GetValue("CategoryId"));
			Assert.AreEqual("http://item.jd.com/3031737.html", results[0].GetValue("Url"));
			Assert.AreEqual("3031737", results[0].GetValue("Sku"));
			Assert.AreEqual("荣耀官方旗舰店", results[0].GetValue("ShopName"));
			Assert.AreEqual("荣耀 NOTE 8 4GB+32GB 全网通版 冰河银", results[0].GetValue("Name"));
			Assert.AreEqual("1000000904", results[0].GetValue("VenderId"));
			Assert.AreEqual("1000000904", results[0].GetValue("JdzyShopId"));
			Assert.AreEqual(DateTime.Now.ToString("yyyy_MM_dd"), results[0].GetValue("RunId"));
		}

		[Table("test", "sku", TableSuffix.Today)]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		public class Product : SpiderEntity
		{
			[PropertyDefine(Expression = "cat", Type = SelectorType.Enviroment)]
			public string CategoryName { get; set; }

			[PropertyDefine(Expression = "cat3", Type = SelectorType.Enviroment)]
			public int CategoryId { get; set; }

			[PropertyDefine(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[PropertyDefine(Expression = "./@data-sku")]
			public string Sku { get; set; }

			[PropertyDefine(Expression = "./div[5]/strong/a")]
			public long CommentsCount { get; set; }

			[PropertyDefine(Expression = ".//div[@class='p-shop']/@data-shop_name")]
			public string ShopName { get; set; }

			[PropertyDefine(Expression = ".//div[@class='p-name']/a/em")]
			public string Name { get; set; }

			[PropertyDefine(Expression = "./@venderid")]
			public string VenderId { get; set; }

			[PropertyDefine(Expression = "./@jdzy_shop_id")]
			public string JdzyShopId { get; set; }

			[PropertyDefine(Expression = "Today", Type = SelectorType.Enviroment)]
			public DateTime RunId { get; set; }
		}
	}
}
