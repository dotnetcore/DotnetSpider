using System;
using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Downloader;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;

namespace DotnetSpider.Sample
{
	public class CasSpider : EntitySpiderBuilder
	{
		protected override EntitySpider GetEntitySpider()
		{
			EntitySpider context = new EntitySpider(new Site())
			{
				Downloader = new HttpClientDownloader
				{
					DownloadCompleteHandlers = new IDownloadCompleteHandler[]
					{
						new IncrementTargetUrlsCreator("index_1.shtml")
					}
				},
			};
			context.SetThreadNum(10);
			context.SetIdentity("qidian_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmss"));
			context.AddEntityPipeline(
				new MySqlEntityPipeline("Database='test';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"));
			context.AddStartUrl("http://www.cas.cn/kx/kpwz/index.shtml");
			context.AddStartUrl("http://www.cas.cn/kx/kpwz/index_1.shtml");
			context.AddEntityType(typeof(ArticleSummary));
			context.AddEntityType(typeof(Article));
			return context;
		}

		[EntitySelector(Expression = "//div[@class='ztlb_ld_mainR_box01_list']/ul/li")]
		[TargetUrlsSelector(Patterns = new[] { @"index_[0-9]+.shtml", "index.shtml" })]
		public class ArticleSummary : ISpiderEntity
		{
			[PropertySelector(Expression = ".//a/@title")]
			public string Title { get; set; }

			[TargetUrl(Extras = new[] { "Title", "Url" })]
			[PropertySelector(Expression = ".//a/@href")]
			public string Url { get; set; }
		}

		[Schema("test", "Article", TableSuffix.Today)]
		[Indexes(Index = new[] { "Title" })]
		[TargetUrlsSelector(Patterns = new[] { @"t[0-9]+_[0-9]+.shtml" })]
		public class Article : ISpiderEntity
		{
			[StoredAs("Title", DataType.String, 100)]
			[PropertySelector(Expression = "Title", Type = SelectorType.Enviroment)]
			public string Title { get; set; }

			[StoredAs("Url", DataType.Text)]
			[PropertySelector(Expression = "Title", Type = SelectorType.Enviroment)]
			public string Url { get; set; }

			[StoredAs("Source", DataType.String, 100)]
			[PropertySelector(Expression = "//span[@id='source']/span[1]")]
			public string Source { get; set; }

			[StoredAs("PublishTime", DataType.String, 20)]
			[PropertySelector(Expression = "/html/body/div[5]/div[1]/div[2]/p/span[2]")]
			public string PublishTime { get; set; }

			[StoredAs("Content", DataType.Text)]
			[PropertySelector(Expression = "//div[@class='TRS_Editor']")]
			public string Content { get; set; }
		}
	}
}
