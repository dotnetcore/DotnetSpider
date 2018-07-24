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
	/// 如果想尝试把数据存到MySql中，请把 AddPipeline(new MySqlEntityPipeline("")); 中的连接字符串替换会您自己MySql的连接字符串
	/// 如果想尝试把数据存到SqlServer中，请使用 AddPipeline(new SqlServerEntityPipeline("")); 中的连接字符串替换会您自己SqlServer的连接字符串
	/// </summary>
	public class EntityModelSpider
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
				AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });
				AddEntityType<BaiduSearchEntry>();
				AddPipeline(new ConsoleEntityPipeline());
			}

			[TableInfo("baidu", "baidu_search_entity_model")]
			[EntitySelector(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
			class BaiduSearchEntry : BaseEntity
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
				[ReplaceFormatter(NewValue = "-", OldValue = "&nbsp;")]
				public string Website { get; set; }

				[FieldSelector(Expression = ".//div/span/a[@class='c-cache']/@href")]
				public string Snapshot { get; set; }

				[FieldSelector(Expression = ".//div[@class='c-summary c-row ']", Option = FieldOptions.InnerText)]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
				public string Details { get; set; }

				[FieldSelector(Expression = ".", Option = FieldOptions.InnerText)]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
				public string PlainText { get; set; }
			}
		}
	}
}
