using DotnetSpider.Common;
using DotnetSpider.Core;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using System.Collections.Generic;

namespace DotnetSpider.Extension
{
	public class SampleSpider
	{
		public static void Run()
		{
			var site = new Site();
			var mode = new ModelDefinition
				(
				new Selector(".//div[@class='result']"),
				new[]
				{
					new FieldSelector("Keyword","Keyword",SelectorType.Enviroment),
					new FieldSelector(".//h3[@class='c-title']/a","Title"),
					new FieldSelector(".//h3[@class='c-title']/a/@href","Url"),
					new FieldSelector(".//div/p[@class='c-author']/text()","Website"),
					new FieldSelector(".//div/span/a[@class='c-cache']/@href","Snapshot"),
					new FieldSelector(".//div[@class='c-summary c-row ']","Details"),
					new FieldSelector(".","PlainText"),
					new FieldSelector( "today","atime", SelectorType.Enviroment,DataType.Date)
				}
				, new TableInfo("baidu", "search"), null);
			var processor = new ModelProcessor(mode);
			var spider = Spider.Create(site, processor);
			var word = "可乐|雪碧";
			spider.AddStartUrl($"http://news.baidu.com/ns?word={word}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", new Dictionary<string, dynamic> { { "Keyword", word } });
			//spider.AddPipeline(new MySqlEntityPipeline());
			spider.Run();
		}
	}
}
