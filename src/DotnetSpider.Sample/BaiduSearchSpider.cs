using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using System.Collections.Generic;

namespace DotnetSpider.Sample
{
	public class BaiduSearchSpider : EntitySpiderBuilder
	{
		public BaiduSearchSpider() : base("BaiduSearch", Batch.Now)
		{
		}

		protected override EntitySpider GetEntitySpider()
		{
			EntitySpider context = new EntitySpider(new Site
			{
				EncodingName = "UTF-8"
			});

			context.AddPipeline(
				new MySqlEntityPipeline("Database='test';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"));

			var word = "可乐|雪碧";
			context.AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });
			context.AddEntityType(typeof(BaiduSearchEntry));

			return context;
		}
	}

	[Table("DB", "BaiduSearch", TableSuffix.Today)]
	[EntitySelector(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
	[TargetUrlsSelector(XPaths = new[] { "//p[@id=\"page\"]" }, Patterns = new[] { @"&pn=[0-9]+&" })]
	public class BaiduSearchEntry : SpiderEntity
	{
		[PropertyDefine(Expression = "Keyword", Type = SelectorType.Enviroment)]
		public string Keyword { get; set; }

		[PropertyDefine(Expression = ".//h3[@class='c-title']/a")]
		[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
		[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
		public string Title { get; set; }

		[PropertyDefine(Expression = ".//h3[@class='c-title']/a/@href")]
		public string Url { get; set; }

		[PropertyDefine(Expression = ".//div/p[@class='c-author']/text()")]
		[ReplaceFormatter(NewValue = "-", OldValue = "&nbsp;")]
		public string Website { get; set; }


		[PropertyDefine(Expression = ".//div/span/a[@class='c-cache']/@href")]
		public string Snapshot { get; set; }


		[PropertyDefine(Expression = ".//div[@class='c-summary c-row ']", Option = PropertyDefine.Options.PlainText)]
		[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
		[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
		[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
		public string Details { get; set; }

		[PropertyDefine(Expression = ".", Option = PropertyDefine.Options.PlainText)]
		[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
		[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
		[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
		public string PlainText { get; set; }

	}
}
