using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Sample
{
	[TaskName("TestSpider")]
	public class TestSpider : EntitySpider
	{
		public TestSpider() : base("TestSpider")
		{
		}

		protected override void MyInit(params string[] arguments)
		{
			var word = "可乐|雪碧";
			AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });
			AddEntityType<BaiduSearchEntry>();
			AddPipeline(new SqlServerEntityPipeline("Server=.\\SQLEXPRESS;Database=master;Trusted_Connection=True;MultipleActiveResultSets=true"));
		}

		[TableInfo("baidu", "baidu_search_entity_model")]
		[EntitySelector(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
		class BaiduSearchEntry : BaseEntity
		{
			[Field(Expression = "Keyword", Type = SelectorType.Enviroment)]
			public string Keyword { get; set; }

			[Field(Expression = ".//h3[@class='c-title']/a")]
			[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
			[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
			public string Title { get; set; }

			[Field(Expression = ".//h3[@class='c-title']/a/@href")]
			public string Url { get; set; }

			[Field(Expression = ".//div/p[@class='c-author']/text()")]
			[ReplaceFormatter(NewValue = "-", OldValue = "&nbsp;")]
			public string Website { get; set; }

			[Field(Expression = ".//div/span/a[@class='c-cache']/@href", Length = 0)]
			public string Snapshot { get; set; }

			[Field(Expression = ".//div[@class='c-summary c-row ']", Option = FieldOptions.InnerText, Length = 0)]
			[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
			[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
			[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
			public string Details { get; set; }

			[Field(Expression = ".", Option = FieldOptions.InnerText, Length = 0)]
			[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
			[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
			[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
			public string PlainText { get; set; }
		}
	}
}
