using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using DotnetSpider.Extraction.Model.Formatter;

namespace DotnetSpider.Sample.docs
{
	public class OneForAllSpider
	{
		public static void Run()
		{
			Spider spider = new Spider();
			spider.Run();
		}

		class Spider : EntitySpider
		{
			protected override void OnInit(params string[] arguments)
			{
				AddRequests("http://www.jd.com/allSort.aspx");
				AddEntityType<Category>();
				AddEntityType<TmpProduct>();
				AddEntityType<JdProduct>();
				AddPipeline(new ConsoleEntityPipeline());
			}

			[Entity(Expression = ".//div[@class='items']//a")]
			class Category : IBaseEntity
			{
				[Field(Expression = ".")]
				public string CategoryName { get; set; }

				[Next(Extras = new[] { "CategoryName" })]
				[RegexAppendFormatter(Pattern = "http://list.jd.com/list.html\\?cat=[0-9]+", AppendValue = "&page=1&trans=1&JL=6_0_0")]
				[Field(Expression = "./@href")]
				public string Url { get; set; }
			}

			[Entity(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
			[Target(XPaths = new[] { "//span[@class=\"p-num\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
			class TmpProduct : IBaseEntity
			{
				[Field(Expression = "CategoryName", Type = SelectorType.Enviroment)]
				public string CategoryName { get; set; }

				[Next(Extras = new[] { "CategoryName", "Sku", "Name", "Url" })]
				[Field(Expression = "./div[@class='p-name']/a[1]/@href")]
				public string Url { get; set; }

				[Field(Expression = ".//div[@class='p-name']/a/em")]
				public string Name { get; set; }

				[Field(Expression = "./@data-sku")]
				public string Sku { get; set; }
			}

			[Target(XPaths = new[] { "//span[@class=\"p-num\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
			[Schema("jd", "jd_product")]
			class JdProduct : IBaseEntity
			{
				[Column(Length = 100)]
				[Field(Expression = "Name", Type = SelectorType.Enviroment)]
				public string Name { get; set; }

				[Column(Length = 100)]
				[Field(Expression = "Sku", Type = SelectorType.Enviroment)]
				[Index("SKU")]
				[Unique("SKU")]
				public string Sku { get; set; }

				[Column()]
				[Field(Expression = "Url", Type = SelectorType.Enviroment)]
				public string Url { get; set; }

				[Column(Length = 100)]
				[Field(Expression = "CategoryName", Type = SelectorType.Enviroment)]
				public string CategoryName { get; set; }

				[Column(Length = 100)]
				[Field(Expression = ".//a[@class='name']")]
				public string ShopName { get; set; }

				[StringFormater(Format = "http:{0}")]
				[Download]
				[Field(Expression = "//*[@class='brand-logo']/a[1]/img[1]/@src")]
				public string Logo { get; set; }
			}
		}
	}
}