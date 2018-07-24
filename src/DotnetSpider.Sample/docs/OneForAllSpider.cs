using DotnetSpider.Extension;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extraction;
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
				AddStartUrl("http://www.jd.com/allSort.aspx");
				AddEntityType<Category>();
				AddEntityType<TmpProduct>();
				AddEntityType<JdProduct>();
				AddPipeline(new ConsoleEntityPipeline());
			}

			[EntitySelector(Expression = ".//div[@class='items']//a")]
			class Category
			{
				[FieldSelector(Expression = ".")]
				public string CategoryName { get; set; }

				[ToNext(Extras = new[] { "CategoryName" })]
				[RegexAppendFormatter(Pattern = "http://list.jd.com/list.html\\?cat=[0-9]+", AppendValue = "&page=1&trans=1&JL=6_0_0")]
				[FieldSelector(Expression = "./@href")]
				public string Url { get; set; }
			}

			[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
			[TargetRequestSelector(XPaths = new[] { "//span[@class=\"p-num\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
			class TmpProduct
			{
				[FieldSelector(Expression = "CategoryName", Type = SelectorType.Enviroment, Length = 100)]
				public string CategoryName { get; set; }

				[ToNext(Extras = new[] { "CategoryName", "Sku", "Name", "Url" })]
				[FieldSelector(Expression = "./div[@class='p-name']/a[1]/@href")]
				public string Url { get; set; }

				[FieldSelector(Expression = ".//div[@class='p-name']/a/em", Length = 100)]
				public string Name { get; set; }

				[FieldSelector(Expression = "./@data-sku", Length = 100)]
				public string Sku { get; set; }
			}

			[TargetRequestSelector(XPaths = new[] { "//span[@class=\"p-num\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
			[TableInfo("jd", "jd_product", Uniques = new[] { "Sku" }, Indexs = new[] { "Sku" })]
			class JdProduct
			{
				[FieldSelector(Expression = "Name", Type = SelectorType.Enviroment, Length = 100)]
				public string Name { get; set; }

				[FieldSelector(Expression = "Sku", Type = SelectorType.Enviroment, Length = 100)]
				public string Sku { get; set; }

				[FieldSelector(Expression = "Url", Type = SelectorType.Enviroment)]
				public string Url { get; set; }

				[FieldSelector(Expression = "CategoryName", Type = SelectorType.Enviroment, Length = 100)]
				public string CategoryName { get; set; }

				[FieldSelector(Expression = ".//a[@class='name']", Length = 100)]
				public string ShopName { get; set; }

				[StringFormater(Format = "http:{0}")]
				[Download]
				[FieldSelector(Expression = "//*[@class='brand-logo']/a[1]/img[1]/@src", IgnoreStore = true)]
				public string Logo { get; set; }
			}
		}
	}
}