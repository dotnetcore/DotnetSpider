using System;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Downloader;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;

namespace DotnetSpider.Sample
{
	public class CasSpider : EntitySpider
	{
		public CasSpider() : base("cas")
		{
		}

		protected override void MyInit(params string[] arguments)
		{
			Identity = ("qidian_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmss"));
			var downloader = new HttpClientDownloader();
			downloader.AddAfterDownloadCompleteHandler(new IncrementTargetUrlsBuilder("index_1.shtml"));
			Downloader = downloader;
			ThreadNum = 1;
			AddStartUrl("http://www.cas.cn/kx/kpwz/index.shtml");
			AddStartUrl("http://www.cas.cn/kx/kpwz/index_1.shtml");
			AddEntityType(typeof(ArticleSummary));
			AddEntityType(typeof(Article));
		}

		[EntitySelector(Expression = "//div[@class='ztlb_ld_mainR_box01_list']/ul/li")]
		[TargetUrlsSelector(Patterns = new[] { @"index_[0-9]+.shtml", "index.shtml" })]
		class ArticleSummary : SpiderEntity
		{
			[PropertyDefine(Expression = ".//a/@title", Length = 100)]
			public string Title { get; set; }

			[LinkToNext(Extras = new[] { "Title", "Url" })]
			[PropertyDefine(Expression = ".//a/@href")]
			public string Url { get; set; }
		}

		[Table("test", "Article", TableSuffix.Today, Indexs = new[] { "Title" })]
		[TargetUrlsSelector(Patterns = new[] { @"t[0-9]+_[0-9]+.shtml" })]
		class Article : SpiderEntity
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
