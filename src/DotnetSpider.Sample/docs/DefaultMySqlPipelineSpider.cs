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
	public class DefaultMySqlPipelineSpider : CustomizedSpider
	{
		public DefaultMySqlPipelineSpider() : base(new Site())
		{
		}

		protected override void MyInit(params string[] arguments)
		{
			var word = "可乐|雪碧";
			AddPipeline(new DefaultMySqlPipeline(Env.DataConnectionString, "baidu", "mysql_baidu_search"));
			AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });

			var processor = new DefaultPageProcessor();
			processor.AddTargetUrlExtractor("//p[@id=\"page\"]", "&pn=[0-9]+&");
			AddPageProcessors(processor);
		}
	}
}
