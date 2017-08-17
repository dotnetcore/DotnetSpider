using System;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Core;

namespace DotnetSpider.Sample
{
	public class JdSpider : EntitySpider
	{
		public JdSpider() : base("JD", new Site())
		{
		}

		[EntitySelector(Expression = ".//div[@class='items']//a")]
		public class Category : SpiderEntity
		{
			[PropertyDefine(Expression = ".")]
			public string CategoryName { get; set; }

			[LinkToNext(Extras = new[] { "CategoryName" })]
			[RegexAppendFormatter(Pattern = "http://list.jd.com/list.html\\?cat=[0-9]+", AppendValue = "&page=1&trans=1&JL=6_0_0")]
			[PropertyDefine(Expression = "./@href")]
			public string Url { get; set; }
		}

		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		[TargetUrlsSelector(XPaths = new[] { "//span[@class=\"p-num\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class TmpProduct : SpiderEntity
		{
			[PropertyDefine(Expression = "CategoryName", Type = SelectorType.Enviroment, Length = 100)]
			public string CategoryName { get; set; }

			[LinkToNext(Extras = new[] { "CategoryName", "Sku", "Name", "Url" })]
			[PropertyDefine(Expression = "./div[@class='p-name']/a[1]/@href")]
			public string Url { get; set; }

			[PropertyDefine(Expression = ".//div[@class='p-name']/a/em", Length = 100)]
			public string Name { get; set; }

			[PropertyDefine(Expression = "./@data-sku", Length = 100)]
			public string Sku { get; set; }
		}

		[TargetUrlsSelector(XPaths = new[] { "//span[@class=\"p-num\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		[Table("jd", "jd_product", Primary = "Sku", Indexs = new[] { "Sku" })]
		public class JdProduct : SpiderEntity
		{
			[PropertyDefine(Expression = "Name", Type = SelectorType.Enviroment, Length = 100)]
			public string Name { get; set; }

			[PropertyDefine(Expression = "Sku", Type = SelectorType.Enviroment, Length = 100)]
			public string Sku { get; set; }

			[PropertyDefine(Expression = "Url", Type = SelectorType.Enviroment)]
			public string Url { get; set; }

			[PropertyDefine(Expression = "CategoryName", Type = SelectorType.Enviroment, Length = 100)]
			public string CategoryName { get; set; }

			[PropertyDefine(Expression = ".//a[@class='name']", Length = 100)]
			public string ShopName { get; set; }

			[FormatStringFormater(Format = "http:{0}")]
			[Download]
			[PropertyDefine(Expression = "//*[@class='brand-logo']/a[1]/img[1]/@src", IgnoreStore = true)]
			public string Logo { get; set; }

			[PropertyDefine(Expression = "Monday", Type = SelectorType.Enviroment)]
			public DateTime RunId { get; set; }
		}


		protected override void MyInit(params string[] arguments)
		{
			Identity = "Cnblog Daliy Tracking " + DateTimeUtils.Monday_Of_Current_Week.ToString("yyyy-MM-dd");
			AddStartUrl("http://www.jd.com/allSort.aspx");
			AddEntityType(typeof(Category));
			AddEntityType(typeof(TmpProduct));
			AddEntityType(typeof(JdProduct));
			AddPipeline(
				new MySqlEntityPipeline("Database='test';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306"));
		}
	}

}