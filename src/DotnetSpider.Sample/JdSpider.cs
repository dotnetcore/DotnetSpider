using System;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Sample
{
	public class JdSpider : EntitySpiderBuilder
	{
		public JdSpider() : base("JD", Batch.Now)
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
			[PropertyDefine(Expression = "CategoryName", Type = SelectorType.Enviroment)]
			public string CategoryName { get; set; }

			[LinkToNext(Extras = new[] { "CategoryName", "Sku", "Name", "Url" })]
			[PropertyDefine(Expression = "./div[@class='p-name']/a[1]/@href")]
			public string Url { get; set; }

			[PropertyDefine(Expression = ".//div[@class='p-name']/a/em")]
			public string Name { get; set; }

			[PropertyDefine(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}

		[TargetUrlsSelector(XPaths = new[] { "//span[@class=\"p-num\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		[Table("jd", "jd_product", Primary = "Sku", Indexs = new[] { "Sku" })]
		public class JdProduct : SpiderEntity
		{
			[PropertyDefine(Expression = "Name", Type = SelectorType.Enviroment)]
			public string Name { get; set; }

			[PropertyDefine(Expression = "Sku", Type = SelectorType.Enviroment)]
			public string Sku { get; set; }

			[PropertyDefine(Expression = "Url", Type = SelectorType.Enviroment)]
			public string Url { get; set; }

			[PropertyDefine(Expression = "CategoryName", Type = SelectorType.Enviroment)]
			public string CategoryName { get; set; }

			[PropertyDefine(Expression = ".//a[@class='name']")]
			public string ShopName { get; set; }

			[FormatStringFormater(Format = "http:{0}")]
			[Download]
			[PropertyDefine(Expression = "//*[@class='brand-logo']/a[1]/img[1]/@src", IgnoreStore = true)]
			public string Logo { get; set; }

			[PropertyDefine(Expression = "Monday", Type = SelectorType.Enviroment)]
			public DateTime RunId { get; set; }
		}

		protected override EntitySpider GetEntitySpider()
		{
			var entitySpider = new EntitySpider(new Site())
			{
				Identity = "Cnblog Daliy Tracking " + DateTimeUtils.Day1OfThisWeek.ToString("yyyy-MM-dd")
			};

			entitySpider.AddStartUrl("http://www.jd.com/allSort.aspx");
			entitySpider.AddEntityType(typeof(Category));
			entitySpider.AddEntityType(typeof(TmpProduct));
			entitySpider.AddEntityType(typeof(JdProduct));
			entitySpider.AddPipeline(
				new MySqlEntityPipeline("Database='test';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306"));
			return entitySpider;
		}
	}

}