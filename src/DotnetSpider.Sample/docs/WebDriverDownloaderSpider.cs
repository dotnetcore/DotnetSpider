using DotnetSpider.Common;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Downloader;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model.Attribute;
using System.Collections.Generic;

namespace DotnetSpider.Sample.docs
{
	public class WebDriverDownloaderSpider : EntitySpider
	{
		protected override void MyInit(params string[] arguments)
		{
			Downloader = new WebDriverDownloader(Browser.Chrome);
			AddStartUrl("http://list.jd.com/list.html?cat=9987,653,655&page=2&JL=6_0_0&ms=5#J_main", new Dictionary<string, object> { { "name", "手机" }, { "cat3", "655" } });
			AddPipeline(new ConsoleEntityPipeline());
			AddEntityType<Product>();
		}

		[TargetRequestSelector(XPaths = new[] { "//span[@class=\"p-num\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		[TableInfo("test", "sku", TableNamePostfix.Today, Indexs = new[] { "CategoryName" }, Uniques = new[] { "CategoryName,Sku", "Sku" })]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		class Product
		{
			[Field(Expression = "name", Type = SelectorType.Enviroment, Length = 20)]
			public string CategoryName { get; set; }

			[Field(Expression = "cat3", Type = SelectorType.Enviroment, Length = 20)]
			public int CategoryId { get; set; }

			[Field(Expression = "./div[1]/a/@href", Length = 20)]
			public string Url { get; set; }

			[Field(Expression = "./@data-sku", Length = 20)]
			public string Sku { get; set; }

			[Field(Expression = "./div[5]/strong/a", Length = 20)]
			public long CommentsCount { get; set; }

			[Field(Expression = ".//div[@class='p-shop']/@data-shop_name", Length = 20)]
			public string ShopName { get; set; }

			[Field(Expression = ".//div[@class='p-name']/a/em", Length = 20)]
			public string Name { get; set; }

			[Field(Expression = "./@venderid", Length = 20)]
			public string VenderId { get; set; }

			[Field(Expression = "./@jdzy_shop_id", Length = 20)]
			public string JdzyShopId { get; set; }
		}
	}
}