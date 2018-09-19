using DotnetSpider.Core;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using DotnetSpider.Extraction.Model.Formatter;
using System.Collections.Generic;

namespace DotnetSpider.Sample
{
	[TaskName("TestSpider")]
	public class TestSpider : EntitySpider
	{
		public TestSpider() : base("TestSpider")
		{
		}

		protected override void OnInit(params string[] arguments)
		{
			var word = "可乐|雪碧";
			AddRequest(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });
			AddEntityType<BaiduSearchEntry>();
			AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Port=3306;SslMode=None;"));
		}

		[Schema("baidu", "baidu_search_entity_model")]
		[Entity(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
		class BaiduSearchEntry : BaseEntity
		{
			[Column]
			[Field(Expression = "Keyword", Type = SelectorType.Enviroment)]
			public string Keyword { get; set; }

			[Column]
			[Field(Expression = ".//h3[@class='c-title']/a")]
			[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
			[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
			public string Title { get; set; }

			[Column]
			[Field(Expression = ".//h3[@class='c-title']/a/@href")]
			public string Url { get; set; }

			[Column]
			[Field(Expression = ".//div/p[@class='c-author']/text()")]
			[ReplaceFormatter(NewValue = "-", OldValue = "&nbsp;")]
			public string Website { get; set; }

			[Column]
			[Field(Expression = ".//div/span/a[@class='c-cache']/@href")]
			public string Snapshot { get; set; }

			[Column]
			[Field(Expression = ".//div[@class='c-summary c-row ']", Option = FieldOptions.InnerText)]
			[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
			[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
			[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
			public string Details { get; set; }

			[Column(0)]
			[Field(Expression = ".", Option = FieldOptions.InnerText)]
			[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
			[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
			[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
			public string PlainText { get; set; }
		}
	}
}
