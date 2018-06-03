using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using System.Collections.Generic;

namespace DotnetSpider.Extension
{
	public class SampleSpider
	{
		public static void Run()
		{
			var site = new Site { };
			var mode = new ModelDefine
				(
				new Selector(".//div[@class='result']"),
				new[]
				{
					new Field("Keyword","Keyword",SelectorType.Enviroment),
					new Field(".//h3[@class='c-title']/a","Title"),
					new Field(".//h3[@class='c-title']/a/@href","Url"),
					new Field(".//div/p[@class='c-author']/text()","Website"),
					new Field(".//div/span/a[@class='c-cache']/@href","Snapshot"),
					new Field(".//div[@class='c-summary c-row ']","Details"),
					new Field(".","PlainText"),
					new Field( "today","atime", SelectorType.Enviroment,DataType.Date)
				}
				, new TableInfo("baidu", "search") { });
			var processor = new ModelProcessor(mode);
			var spider = Spider.Create(site, processor);
			var word = "可乐|雪碧";
			spider.AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });
			spider.AddPipeline(new MySqlEntityPipeline());
			spider.Run();
		}
	}
}
