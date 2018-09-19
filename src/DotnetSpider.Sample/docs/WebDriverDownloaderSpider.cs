using DotnetSpider.Extension;
using DotnetSpider.Extension.Downloader;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using System.Collections.Generic;

namespace DotnetSpider.Sample.docs
{
	public class WebDriverDownloaderSpider : EntitySpider
	{
		protected override void OnInit(params string[] arguments)
		{
			Downloader = new WebDriverDownloader(Browser.Chrome);
			AddRequest("http://list.jd.com/list.html?cat=9987,653,655&page=2&JL=6_0_0&ms=5#J_main", new Dictionary<string, object> { { "name", "手机" }, { "cat3", "655" } });
			AddPipeline(new ConsoleEntityPipeline());
			AddEntityType<Product>();
		}

		[Target(XPaths = new[] { "//span[@class=\"p-num\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		[Schema("test","sku",TableNamePostfix = TableNamePostfix.Today)]
		[Entity(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		class Product : IBaseEntity
		{
			[Field(Expression = "name", Type = SelectorType.Enviroment)]
			[Index("CAT")]
			[Unique("CAT_SKU")]
			[Column(Length =20)]
			public string CategoryName { get; set; }

			[Field(Expression = "cat3", Type = SelectorType.Enviroment)]
			[Column(Length = 20)]
			public int CategoryId { get; set; }

			[Field(Expression = "./div[1]/a/@href")]
			[Column(Length = 20)]
			public string Url { get; set; }

			[Field(Expression = "./@data-sku")]
			[Column(Length = 20)]
			[Unique("CAT_SKU")]
			[Unique("SKU")]
			public string Sku { get; set; }

			[Field(Expression = "./div[5]/strong/a")]
			[Column()]
			public long CommentsCount { get; set; }

			[Field(Expression = ".//div[@class='p-shop']/@data-shop_name")]
			[Column(Length = 200)]
			public string ShopName { get; set; }

			[Field(Expression = ".//div[@class='p-name']/a/em")]
			[Column(Length = 20)]
			public string Name { get; set; }

			[Field(Expression = "./@venderid")]
			[Column(Length = 20)]
			public string VenderId { get; set; }

			[Field(Expression = "./@jdzy_shop_id")]
			[Column(Length = 20)]
			public string JdzyShopId { get; set; }
		}
	}
}