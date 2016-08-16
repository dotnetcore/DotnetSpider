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
			[RegexAppendFormatter(Pattern = "http://list.jd.com/list.html\\?cat=[0-9]+", Append = "&page=1&trans=1&JL=6_0_0")]
			[PropertySelector(Expression = "./@href")]
			public string Url { get; set; }
		}

		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		public class Product : ISpiderEntity
		{
			[PropertySelector(Expression = "CategoryName", Type = SelectorType.Enviroment)]
			public string CategoryName { get; set; }

			[TargetUrl(Extras = new[] { "CategoryName" })]
			[PropertySelector(Expression = "./div[@class='p-name']/a[1]/@href")]
			public string Url { get; set; }
		}

		[Schema("jd", "jdShop")]
		public class JdShop : ISpiderEntity
		{
			[StoredAs("ShopName", DataType.String, 20)]
			[PropertySelector(Expression = ".//a[@class='name']")]
			public string ShopName { get; set; }

			[StringFormatFormater(Format = "http:{0}")]
			[Download]
			[PropertySelector(Expression = "//*[@class='brand-logo']/a[1]/img[1]/@src")]
			public string Logo { get; set; }

			[StoredAs("run_id", DataType.Date)]
			[PropertySelector(Expression = "Monday", Type = SelectorType.Enviroment)]
			public DateTime RunId { get; set; }
		}

		protected override EntitySpider GetEntitySpider()
		{
			var entitySpider = new EntitySpider(new Site
			{
				EncodingName = "UTF-8"
			})
			{
				Identity = "Cnblog Daliy Tracking " + DateTimeUtils.FirstDayofThisWeek.ToString("yyyy-MM-dd")
			};

			entitySpider.AddStartUrl("http://www.jd.com/allSort.aspx");
			entitySpider.AddEntityType(typeof(Category));
			entitySpider.AddEntityType(typeof(Product), new TargetUrlExtractor
			{
				Region = new BaseSelector { Type = SelectorType.XPath, Expression = "//span[@class=\"p-num\"]" },
				Patterns = new List<string> { @"&page=[0-9]+&" }
			});
			entitySpider.AddEntityType(typeof(JdShop), new TargetUrlExtractor
			{
				Region = new BaseSelector { Type = SelectorType.XPath, Expression = "//span[@class=\"p-num\"]" },
				Patterns = new List<string> { @"http://item\.jd\.com/[0-9]+\.html" }
			});
			entitySpider.AddEntityPipeline(
				new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"));
			return entitySpider;
		}
	}

}