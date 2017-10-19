using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using System;
using System.Collections.Generic;

namespace DotnetSpider.Sample
{
	[Properties(Owner = "Fay", Developer = "Lewis", Date = "2017-07-27", Subject = "百度搜索结果", Email = "136831898@qq.com")]
	[TaskName("BaiduSearch")]
	public class BaiduSearchSpider : EntitySpider
	{
		public BaiduSearchSpider() : base("BaiduSearch")
		{
		}

		protected override void MyInit(params string[] arguments)
		{
			var word = "可乐|雪碧";
			AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });

			AddEntityType<BaiduSearchEntry>();
			DataVerificationAndReport += () =>
			{
				Verification<BaiduSearchSpider> verifier = new Verification<BaiduSearchSpider>();
				verifier.AddSqlEqual("采集总量", "SELECT COUNT(*) AS Result baidu.baidu_search WHERE run_id = DATE(); ", 10);
				verifier.Report();
			};
		}

		[EntityTable("baidu", "baidu_search")]
		[EntitySelector(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
		class BaiduSearchEntry : SpiderEntity
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

			[PropertyDefine(Expression = "today", Type = SelectorType.Enviroment)]
			public DateTime run_id { get; set; }
		}
	}
}
