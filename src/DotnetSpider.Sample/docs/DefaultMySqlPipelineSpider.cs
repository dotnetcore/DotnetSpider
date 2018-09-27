using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Processor.Filter;
using DotnetSpider.Core.Processor.RequestExtractor;
using DotnetSpider.Extension.Pipeline;
using System.Collections.Generic;

namespace DotnetSpider.Sample.docs
{
	public class DefaultMySqlPipelineSpider : Spider
	{
		public DefaultMySqlPipelineSpider()
		{
		}

		protected override void OnInit(params string[] arguments)
		{
			var word = "可乐|雪碧";
			AddPipeline(new DefaultMySqlPipeline(Env.DataConnectionString, "baidu", "mysql_baidu_search"));
			AddRequest(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });

			var processor = new DefaultPageProcessor();
			processor.RequestExtractor = new XPathRequestExtractor("//p[@id=\"page\"]");
			processor.Filter = new PatternFilter(new[] { "&pn=[0-9]+&" });
			AddPageProcessors(processor);
		}
	}
}
