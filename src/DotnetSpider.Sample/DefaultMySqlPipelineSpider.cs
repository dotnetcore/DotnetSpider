using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using System;
using System.Collections.Generic;

namespace DotnetSpider.Sample
{
	public class DefaultMySqlPipelineSpider : CommonSpider
	{
		public DefaultMySqlPipelineSpider() : base("DefaultMySqlPipeline")
		{
		}

		protected override void MyInit(params string[] arguments)
		{
			var word = "可乐|雪碧";
			AddPipeline(new DefaultMySqlPipeline(Core.Environment.DataConnectionString, "baidu", "mysql_baidu_search"));
			AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });

			var processor = new DefaultPageProcessor();
			processor.AddTargetUrlExtractor("//p[@id=\"page\"]", "&pn=[0-9]+&");
			AddPageProcessor(processor);
		}
	}
}
