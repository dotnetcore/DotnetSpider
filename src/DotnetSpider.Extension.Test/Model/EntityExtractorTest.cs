using System;
using System.Collections.Generic;
using System.IO;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;

using Xunit;
using DotnetSpider.Extension.Processor;
using System.Linq;

namespace DotnetSpider.Extension.Test.Model
{

	public class EntityExtractorTest
	{
		[Fact(DisplayName = "Extract")]
		public void Extract()
		{
			ModelExtractor<Product> extractor = new ModelExtractor<Product>();
			var mode = new ModelDefine<Product>();
			var results = extractor.Extract(new Page(new Request("http://list.jd.com/list.html?cat=9987,653,655&page=2&JL=6_0_0&ms=5#J_main", new Dictionary<string, dynamic>
			{
				{ "cat", "手机" },
				{ "cat3", "110" }
			})
			{ Site = new Site() })
			{
				Content = File.ReadAllText(Path.Combine(Env.BaseDirectory, "Jd.html"))
			}, mode);
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

		[Fact(DisplayName = "TempEntityNoPrimaryInfo")]
		public void TempEntityNoPrimaryInfo()
		{
			EntityProcessor<Entity1> processor = new EntityProcessor<Entity1>();
			var page = new Page(new Request("http://www.abcd.com") { Site = new Site() })
			{
				Content = "{'data':[{'age':'1'},{'age':'2'}]}"
			};
			processor.Process(page, new DefaultSpider());
			Assert.Equal(2, (page.ResultItems.GetResultItem($"DotnetSpider.Extension.Test.Model.EntityExtractorTest+Entity1").Item2).Count);
		}

		[TableInfo("test", "sku2", TableNamePostfix.Today)]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		private class Product
		{
			public string AAA;
			private string bb;

			[Field(Expression = "cat", Type = SelectorType.Enviroment)]
			public string CategoryName { get; set; }

			[Field(Expression = "cat3", Type = SelectorType.Enviroment)]
			public int CategoryId { get; set; }

			[Field(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[Field(Expression = "./@data-sku")]
			public string Sku { get; set; }

			[Field(Expression = "./div[5]/strong/a")]
			public long CommentsCount { get; set; }

			[Field(Expression = ".//div[@class='p-shop']/@data-shop_name")]
			public string ShopName { get; set; }

			[Field(Expression = ".//div[@class='p-name']/a/em")]
			public string Name { get; set; }

			[Field(Expression = "./@venderid")]
			public string VenderId { get; set; }

			[Field(Expression = "./@jdzy_shop_id")]
			public string JdzyShopId { get; set; }

			[Field(Expression = "Today", Type = SelectorType.Enviroment)]
			public DateTime RunId { get; set; }
		}

		[EntitySelector(Expression = "$.data[*]", Type = SelectorType.JsonPath)]
		private class Entity1
		{
			[Field(Expression = "$.age", Type = SelectorType.JsonPath)]
			public int Age { get; set; }
		}
	}
}
