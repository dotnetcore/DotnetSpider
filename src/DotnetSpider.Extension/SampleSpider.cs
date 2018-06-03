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
				new Selector(SelectorType.XPath, ".//div[@class='result']"),
				new TableInfo("baidu", "search") { },
				new[]
				{
					new Field(SelectorType.Enviroment,"Keyword","Keyword"),
					new Field(SelectorType.XPath,".//h3[@class='c-title']/a","Title"),
					new Field(SelectorType.XPath,".//h3[@class='c-title']/a/@href","Url"),
					new Field(SelectorType.XPath,".//div/p[@class='c-author']/text()","Website"),
					new Field(SelectorType.XPath,".//div/span/a[@class='c-cache']/@href","Snapshot"),
					new Field(SelectorType.XPath,".//div[@class='c-summary c-row ']","Details"),
					new Field(SelectorType.XPath,".","PlainText"),
					new Field(SelectorType.Enviroment, "today","atime", DataType.Date)
				});
			var processor = new ModelProcessor(mode);
			var spider = Spider.Create(site, processor);
			var word = "可乐|雪碧";
			spider.AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });
			spider.AddPipeline(new MySqlEntityPipeline());
			spider.Run();
		}
	}
}
