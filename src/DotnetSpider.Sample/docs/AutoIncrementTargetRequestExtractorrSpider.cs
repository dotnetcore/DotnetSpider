using DotnetSpider.Common;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Processor.TargetRequestExtractors;
using DotnetSpider.Downloader;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DotnetSpider.Sample.docs
{
	public class AutoIncrementTargetRequestExtractorrSpider
	{
		public static void Run()
		{
			CnblogsSpider spider = new CnblogsSpider();
			spider.Run();
		}

		private class CnblogsSpider : EntitySpider
		{
			public CnblogsSpider() : base(new Site())
			{
			}

			protected override void MyInit(params string[] arguments)
			{
				Identity = ("cnblogs_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmss"));
				AddStartUrl("https://news.cnblogs.com/n/page/1");
				AddPipeline(new ConsoleEntityPipeline());
				AddEntityType<News>(new AutoIncrementTargetRequestExtractor("page/1"));
			}

			[EntitySelector(Expression = "//div[@class='news_block']")]
			[TableInfo("cnblogs", "news")]
			class News : BaseEntity
			{
				[Field(Expression = ".//h2[@class='news_entry']")]
				public string Name { get; set; }

				[Field(Expression = ".//span[@class='view']")]
				public string View { get; set; }
			}
		}
	}
}
