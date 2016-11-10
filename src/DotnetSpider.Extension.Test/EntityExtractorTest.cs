using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using Xunit;

namespace DotnetSpider.Extension.Test
{
	public class EntityExtractorTest
	{
		[Fact]
		public void Extract()
		{
			var entityMetadata = EntitySpider.ParseEntityMetaData(typeof(Product).GetTypeInfo());
			EntityExtractor extractor = new EntityExtractor("test", null, entityMetadata);
			var results = extractor.Extract(new Page(new Request("http://list.jd.com/list.html?cat=9987,653,655&page=2&JL=6_0_0&ms=5#J_main", 1, new Dictionary<string, dynamic>
			{
				{ "cat", "手机" },
				{ "cat3", "110" }
			}), ContentType.Html, null)
			{
				Content = File.ReadAllText(Path.Combine(SpiderConsts.BaseDirectory, "Jd.html"))
			});
			Assert.Equal(60, results.Count);
			Assert.Equal("手机", results[0].GetValue("CategoryName"));
			Assert.Equal("110", results[0].GetValue("CategoryId"));
			Assert.Equal("http://item.jd.com/3031737.html", results[0].GetValue("Url"));
			Assert.Equal("3031737", results[0].GetValue("Sku"));
			Assert.Equal("荣耀官方旗舰店", results[0].GetValue("ShopName"));
			Assert.Equal("荣耀 NOTE 8 4GB+32GB 全网通版 冰河银", results[0].GetValue("Name"));
			Assert.Equal("1000000904", results[0].GetValue("VenderId"));
			Assert.Equal("1000000904", results[0].GetValue("JdzyShopId"));
			Assert.Equal(DateTime.Now.ToString("yyyy-MM-dd"), results[0].GetValue("RunId"));
		}

		[Schema("test", "sku", TableSuffix.Today)]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		public class Product : ISpiderEntity
		{
			[PropertySelector(Expression = "cat", Type = SelectorType.Enviroment)]
			public string CategoryName { get; set; }

			[PropertySelector(Expression = "cat3", Type = SelectorType.Enviroment)]
			public int CategoryId { get; set; }

			[PropertySelector(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[PropertySelector(Expression = "./@data-sku")]
			public string Sku { get; set; }

			[PropertySelector(Expression = "./div[5]/strong/a")]
			public long CommentsCount { get; set; }

			[PropertySelector(Expression = ".//div[@class='p-shop']/@data-shop_name")]
			public string ShopName { get; set; }

			[PropertySelector(Expression = ".//div[@class='p-name']/a/em")]
			public string Name { get; set; }

			[PropertySelector(Expression = "./@venderid")]
			public string VenderId { get; set; }

			[PropertySelector(Expression = "./@jdzy_shop_id")]
			public string JdzyShopId { get; set; }

			[PropertySelector(Expression = "Today", Type = SelectorType.Enviroment)]
			public DateTime RunId { get; set; }

			[PropertySelector(Expression = "Now", Type = SelectorType.Enviroment)]
			public DateTime CDate { get; set; }
		}
	}
}
