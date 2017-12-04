﻿using System;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;

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
		class Category : SpiderEntity
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
		class TmpProduct : SpiderEntity
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
		[EntityTable("DotnetSpider", "jd_product", Uniques = new[] { "Sku" }, Indexs = new[] { "Sku" })]
		class JdProduct : SpiderEntity
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

			[StringFormater(Format = "http:{0}")]
			[Download]
			[PropertyDefine(Expression = "//*[@class='brand-logo']/a[1]/img[1]/@src", IgnoreStore = true)]
			public string Logo { get; set; }

			[PropertyDefine(Expression = "Monday", Type = SelectorType.Enviroment)]
			public DateTime RunId { get; set; }
		}


		protected override void MyInit(params string[] arguments)
		{
			Identity = "Cnblog Daliy Tracking " + DateTime.Now.ToString("yyyy-MM-dd");
			AddStartUrl("http://www.jd.com/allSort.aspx");
			AddEntityType<Category>();
			AddEntityType<TmpProduct>();
			AddEntityType<JdProduct>();
			AddPipeline(new MySqlEntityPipeline( Env.DataConnectionString));
		}
	}

}