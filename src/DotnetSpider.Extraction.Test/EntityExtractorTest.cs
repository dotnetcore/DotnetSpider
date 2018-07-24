using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using System.Linq;
using DotnetSpider.Extraction.Model.Attribute;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;

namespace DotnetSpider.Extension.Test.Model
{

	public class EntityExtractorTest
	{
		[Fact(DisplayName = "Extract")]
		public void Extract()
		{
			ModelExtractor<Product> extractor = new ModelExtractor<Product>();
			var model = new ModelDefinition<Product>();
			var selectable = new Selectable(
				File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Jd.html")), "http://jd.com", "");
			selectable.Properties = new Dictionary<string, dynamic> {
				{ "cat", "手机" },
				{ "cat3", "110" }
			};

			var results = extractor.Extract(selectable, model);
			Assert.Equal(60, results.Count());
			Assert.Equal("手机", results.First().CategoryName);
			Assert.Equal(110, results.First().CategoryId);
			Assert.Equal("http://item.jd.com/3031737.html", results.First().Url);
			Assert.Equal("3031737", results.First().Sku);
			Assert.Equal("荣耀官方旗舰店", results.First().ShopName);
			Assert.Equal("荣耀 NOTE 8 4GB+32GB 全网通版 冰河银", results.First().Name);
			Assert.Equal("1000000904", results.First().VenderId);
			Assert.Equal("1000000904", results.First().JdzyShopId);
			Assert.Equal(DateTime.Now.ToString("yyyy-MM-dd"), results.First().RunId.ToString("yyyy-MM-dd"));
		}


		[TableInfo("test", "sku2", TableNamePostfix.Today)]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		private class Product
		{
			public string AAA;
			private string bb;

			[FieldSelector(Expression = "cat", Type = SelectorType.Enviroment)]
			public string CategoryName { get; set; }

			[FieldSelector(Expression = "cat3", Type = SelectorType.Enviroment)]
			public int CategoryId { get; set; }

			[FieldSelector(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[FieldSelector(Expression = "./@data-sku")]
			public string Sku { get; set; }

			[FieldSelector(Expression = "./div[5]/strong/a")]
			public long CommentsCount { get; set; }

			[FieldSelector(Expression = ".//div[@class='p-shop']/@data-shop_name")]
			public string ShopName { get; set; }

			[FieldSelector(Expression = ".//div[@class='p-name']/a/em")]
			public string Name { get; set; }

			[FieldSelector(Expression = "./@venderid")]
			public string VenderId { get; set; }

			[FieldSelector(Expression = "./@jdzy_shop_id")]
			public string JdzyShopId { get; set; }

			[FieldSelector(Expression = "Today", Type = SelectorType.Enviroment)]
			public DateTime RunId { get; set; }
		}
	}
}
