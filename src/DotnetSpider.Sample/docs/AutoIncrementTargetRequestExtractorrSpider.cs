using DotnetSpider.Common;
using DotnetSpider.Core.Processor.TargetRequestExtractors;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using System;

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

			protected override void OnInit(params string[] arguments)
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
				[FieldSelector(Expression = ".//h2[@class='news_entry']")]
				public string Name { get; set; }

				[FieldSelector(Expression = ".//span[@class='view']")]
				public string View { get; set; }
			}
		}
	}
}
