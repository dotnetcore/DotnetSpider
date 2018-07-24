using DotnetSpider.Common;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using DotnetSpider.Extraction.Model.Formatter;
using System.Collections.Generic;

namespace DotnetSpider.Sample.docs
{
	/// <summary>
	/// After spider complete, check the data in mysql, website are null in every row.
	/// </summary>
	public class CustomizeFormatterSpider
	{
		public static void Run()
		{
			BaiduSearchSpider spider = new BaiduSearchSpider();
			spider.Run();
		}

		[TaskName("baidu_search")]
		class BaiduSearchSpider : EntitySpider
		{
			protected override void OnInit(params string[] arguments)
			{
				EmptySleepTime = 1000;
				var word = "可乐|雪碧";
				AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });
				AddEntityType<Result>();
				AddPipeline(new ConsoleEntityPipeline());
			}

			[TableInfo("baidu", "baidu_search_customizeforamtter")]
			[EntitySelector(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
			class Result : BaseEntity
			{
				[FieldSelector(Expression = "Keyword", Type = SelectorType.Enviroment)]
				public string Keyword { get; set; }

				[FieldSelector(Expression = ".//h3[@class='c-title']/a")]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				public string Title { get; set; }

				[FieldSelector(Expression = ".//h3[@class='c-title']/a/@href")]
				public string Url { get; set; }

				[FieldSelector(Expression = ".//div/p[@class='c-author']/text()")]
				[NullFormatter]
				public string Website { get; set; }
			}
		}
	}

	public class NullFormatter : Formatter
	{
		protected override void CheckArguments()
		{
		}

		protected override object FormateValue(object value)
		{
			return "";
		}
	}
}
