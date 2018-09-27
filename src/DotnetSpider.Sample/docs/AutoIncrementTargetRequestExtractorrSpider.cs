using DotnetSpider.Core.Processor.RequestExtractor;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
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
			public CnblogsSpider()
			{
			}

			protected override void OnInit(params string[] arguments)
			{
				Identity = ("cnblogs_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmss"));
				AddRequests("https://news.cnblogs.com/n/page/1");
				AddPipeline(new ConsoleEntityPipeline());
				AddEntityType<News>().SetRequestExtractor(new AutoIncrementRequestExtractor("page/1"));
			}

			[Entity(Expression = "//div[@class='news_block']")]
			[Schema("cnblogs", "news")]
			class News : BaseEntity
			{
				[Column]
				[Field(Expression = ".//h2[@class='news_entry']")]
				public string Name { get; set; }

				[Column]
				[Field(Expression = ".//span[@class='view']")]
				public string View { get; set; }
			}
		}
	}
}
