using System;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Downloader;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Sample
{
	public class CasSpider : EntitySpiderBuilder
	{
		public CasSpider() : base("CasSpider", Batch.Now)
		{
		}

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
			context.AddPipeline(
				new MySqlEntityPipeline("Database='test';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"));
			context.AddStartUrl("http://www.cas.cn/kx/kpwz/index.shtml");
			context.AddStartUrl("http://www.cas.cn/kx/kpwz/index_1.shtml");
			context.AddEntityType(typeof(ArticleSummary));
			context.AddEntityType(typeof(Article));
			return context;
		}

		[EntitySelector(Expression = "//div[@class='ztlb_ld_mainR_box01_list']/ul/li")]
		[TargetUrlsSelector(Patterns = new[] { @"index_[0-9]+.shtml", "index.shtml" })]
		public class ArticleSummary : SpiderEntity
		{
			[PropertyDefine(Expression = ".//a/@title")]
			public string Title { get; set; }

			[LinkToNext(Extras = new[] { "Title", "Url" })]
			[PropertyDefine(Expression = ".//a/@href")]
			public string Url { get; set; }
		}

		[Table("test", "Article", TableSuffix.Today, Indexs = new[] { "Title" })]
		[TargetUrlsSelector(Patterns = new[] { @"t[0-9]+_[0-9]+.shtml" })]
		public class Article : SpiderEntity
		{
			[PropertyDefine(Expression = "Title", Type = SelectorType.Enviroment, Length = 100)]
			public string Title { get; set; }

			[PropertyDefine(Expression = "Title", Type = SelectorType.Enviroment)]
			public string Url { get; set; }

			[PropertyDefine(Expression = "//span[@id='source']/span[1]", Length = 100)]
			public string Source { get; set; }

			[PropertyDefine(Expression = "/html/body/div[5]/div[1]/div[2]/p/span[2]", Length = 20)]
			public string PublishTime { get; set; }

			[PropertyDefine(Expression = "//div[@class='TRS_Editor']")]
			public string Content { get; set; }
		}
	}
}
