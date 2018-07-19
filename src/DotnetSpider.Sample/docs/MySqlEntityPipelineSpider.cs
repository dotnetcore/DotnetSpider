using DotnetSpider.Common;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Infrastructure.Database;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Processor.TargetRequestExtractors;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Downloader;
using DotnetSpider.Downloader.AfterDownloadCompleteHandlers;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using DotnetSpider.Extraction.Model.Formatter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
			protected override void MyInit(params string[] arguments)
			{
				var word = "可乐|雪碧";
				AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });
				AddEntityType<Result>();
				AddPipeline(new MySqlEntityPipeline());
			}

			[TableInfo("baidu", "baidu_search_result")]
			[EntitySelector(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
			class Result : BaseEntity
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

				[Field(Expression = ".//div/span/a[@class='c-cache']/@href")]
				public string Snapshot { get; set; }

				[Field(Expression = ".//div[@class='c-summary c-row ']", Option = FieldOptions.InnerText)]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
				public string Details { get; set; }

				[Field(Expression = ".", Option = FieldOptions.InnerText)]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
				public string PlainText { get; set; }
			}
		}
	}
}
