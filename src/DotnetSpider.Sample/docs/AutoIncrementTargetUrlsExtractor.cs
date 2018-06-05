using DotnetSpider.Core;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Sample.docs
{
	public class AutoIncrementTargetUrlsExtractor
	{
		public static void Run()
		{
			CnblogsSpider spider = new CnblogsSpider();
			spider.Run();
		}

		private class CnblogsSpider : EntitySpider
		{
			public CnblogsSpider() : base("cas", new Site { SleepTime = 1000 })
			{
			}

			protected override void MyInit(params string[] arguments)
			{
				Identity = ("cnblogs_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmss"));
				AddStartUrl("https://news.cnblogs.com/n/page/1");
				AddPipeline(new ConsoleEntityPipeline());
				AddEntityType<News>(new Core.Processor.AutoIncrementTargetUrlsExtractor("page/1"));
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
