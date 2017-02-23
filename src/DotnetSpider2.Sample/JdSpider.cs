using System;
using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Common;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Core.Proxy;

namespace DotnetSpider.Sample
{
	public class JdSpider : EntitySpiderBuilder
	{
		[EntitySelector(Expression = ".//div[@class='items']//a")]
		public class Category : ISpiderEntity
		{
			[PropertySelector(Expression = ".")]
			public string CategoryName { get; set; }

			[TargetUrl(Extras = new[] { "CategoryName" })]
			[RegexAppendFormatter(Pattern = "http://list.jd.com/list.html\\?cat=[0-9]+", AppendValue = "&page=1&trans=1&JL=6_0_0")]
			[PropertySelector(Expression = "./@href")]
			public string Url { get; set; }
		}

		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		[TargetUrlsSelector(XPaths = new[] { "//span[@class=\"p-num\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class TmpProduct : ISpiderEntity
		{
			[PropertySelector(Expression = "CategoryName", Type = SelectorType.Enviroment)]
			public string CategoryName { get; set; }

			[TargetUrl(Extras = new[] { "CategoryName", "Sku", "Name", "Url" })]
			[PropertySelector(Expression = "./div[@class='p-name']/a[1]/@href")]
			public string Url { get; set; }

			[PropertySelector(Expression = ".//div[@class='p-name']/a/em")]
			public string Name { get; set; }

			[PropertySelector(Expression = "./@data-sku")]
			public string Sku { get; set; }
		}

		[TargetUrlsSelector(XPaths = new[] { "//span[@class=\"p-num\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		[Schema("jd", "jd_product")]
		[Indexes(Index = new[] { "Sku" }, Primary = "Sku")]
		public class JdProduct : ISpiderEntity
		{
			[StoredAs("Name", DataType.String, 50)]
			[PropertySelector(Expression = "Name", Type = SelectorType.Enviroment)]
			public string Name { get; set; }

			[StoredAs("Sku", DataType.String, 25)]
			[PropertySelector(Expression = "Sku", Type = SelectorType.Enviroment)]
			public string Sku { get; set; }

			[StoredAs("Url", DataType.Text)]
			[PropertySelector(Expression = "Url", Type = SelectorType.Enviroment)]
			public string Url { get; set; }

			[StoredAs("CategoryName", DataType.String, 20)]
			[PropertySelector(Expression = "CategoryName", Type = SelectorType.Enviroment)]
			public string CategoryName { get; set; }

			[StoredAs("ShopName", DataType.String, 20)]
			[PropertySelector(Expression = ".//a[@class='name']")]
			public string ShopName { get; set; }

			[FormatStringFormater(Format = "http:{0}")]
			[Download]
			[PropertySelector(Expression = "//*[@class='brand-logo']/a[1]/img[1]/@src")]
			public string Logo { get; set; }

			[StoredAs("run_id", DataType.Date)]
			[PropertySelector(Expression = "Monday", Type = SelectorType.Enviroment)]
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
			entitySpider.AddEntityPipeline(
				new MySqlEntityPipeline("Database='test';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306"));
			return entitySpider;
		}
	}

}