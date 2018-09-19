using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using DotnetSpider.Extraction.Model.Formatter;
using System.Collections.Generic;

namespace DotnetSpider.Sample.docs
{
	public class MySqlEntityPipelineSpider
	{
		public static void Run()
		{
			Spider spider = new Spider();
			spider.Run();
		}

		private class Spider : EntitySpider
		{
			protected override void OnInit(params string[] arguments)
			{
				var word = "可乐|雪碧";
				AddRequest(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });
				AddEntityType<Result>();
				AddPipeline(new MySqlEntityPipeline());
			}

			[Entity(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
			[Schema("baidu", "baidu_search_result")]
			class Result : BaseEntity
			{
				[Field(Expression = "Keyword", Type = SelectorType.Enviroment)]
				[Column]
				public string Keyword { get; set; }

				[Field(Expression = ".//h3[@class='c-title']/a")]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				[Column]
				public string Title { get; set; }

				[Field(Expression = ".//h3[@class='c-title']/a/@href")]
				[Column]
				public string Url { get; set; }

				[Field(Expression = ".//div/p[@class='c-author']/text()")]
				[ReplaceFormatter(NewValue = "-", OldValue = "&nbsp;")]
				[Column]
				public string Website { get; set; }

				[Field(Expression = ".//div/span/a[@class='c-cache']/@href")]
				[Column]
				public string Snapshot { get; set; }

				[Column]
				[Field(Expression = ".//div[@class='c-summary c-row ']", Option = FieldOptions.InnerText)]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
				public string Details { get; set; }

				[Column(Length = 0)]
				[Field(Expression = ".", Option = FieldOptions.InnerText)]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
				public string PlainText { get; set; }
			}
		}
	}
}
