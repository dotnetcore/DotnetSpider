using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Processor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetSpider.Sample
{
	[TaskName("MultiSupplementSpider")]
	public class MultiSupplementSpider : EntitySpider
	{
		private class MyDownloader : BaseDownloader
		{
			protected override Task<Page> DowloadContent(Request request, ISpider spider)
			{
				Page page;
				if (request.Url.Contains("a.com"))
				{
					page = new Page(request)
					{
						Content = "{\"a\": \"a\"}"
					};
				}
				if (request.Url.Contains("b.com"))
				{
					page = new Page(request)
					{
						Content = "{\"b\": \"b\"}"
					};
				}
				page = new Page(request)
				{
					Content = "{}"
				};
				return Task.FromResult(page);
			}
		}

		private class MyExtractor : ModelExtractor
		{

			public override IEnumerable<dynamic> Extract(Page page, IModel model)
			{

				Dictionary<string, dynamic> tmp = new Dictionary<string, dynamic>();

				// 通过规则判断数据解析
				if (page.Request.Url.Contains("a.com"))
				{
					tmp["a"] = page.Selectable.JsonPath("$.a").GetValue();
				}
				// 通过规则判断数据解析
				if (page.Request.Url.Contains("b.com"))
				{
					tmp["b"] = page.Selectable.JsonPath("$.b").GetValue();
				}


				// 当返回空结果时, 爬虫可以忽略不运行Pipeline, 即运行也不会插入数据
				return new[] { tmp };
			}
		}

		protected override void MyInit(params string[] arguments)
		{
			SkipTargetUrlsWhenResultIsEmpty = false;

			AddStartUrl("http://a.com");

			Downloader = new MyDownloader();
			var process = new EntityProcessor<MyEntity>(new MyExtractor());
			AddPageProcessors(process);
		}


		[TableInfo("test", "MultiSupplement")]
		private class MyEntity
		{
			[Field(Expression = "a")]
			public string a { get; set; }

			[Field(Expression = "b")]
			public string b { get; set; }
		}
	}
}
