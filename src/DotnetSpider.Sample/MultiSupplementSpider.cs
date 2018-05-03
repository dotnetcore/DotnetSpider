using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		private class MyExtractor : BaseEntityExtractor<MyEntity>
		{
			private readonly string _key = "myEntity";

			public override IEnumerable<MyEntity> Extract(Page page)
			{
				MyEntity enity = page.Request.GetExtra(_key);
				if (enity == null)
				{
					enity = new MyEntity();
				}
				// 通过规则判断数据解析
				if (page.Request.Url.Contains("a.com"))
				{
					enity.a = page.Selectable.JsonPath("$.a").GetValue();
				}
				// 通过规则判断数据解析
				if (page.Request.Url.Contains("b.com"))
				{
					enity.b = page.Selectable.JsonPath("$.b").GetValue();
				}
				// 通过规则判断数据是否完整, 只有数据完整时才解析成功
				if (enity.a != null && enity.b != null)
				{
					return new[] { enity };
				}
				else
				{
					// 构造补充链接
					if (enity.b == null)
					{
						var request = new Request("http://b.com");
						request.PutExtra(_key, enity);
						page.AddTargetRequest(request);
					}
				}

				// 当返回空结果时, 爬虫可以忽略不运行Pipeline, 即运行也不会插入数据
				return Enumerable.Empty<MyEntity>();
			}
		}

		protected override void MyInit(params string[] arguments)
		{
			SkipTargetUrlsWhenResultIsEmpty = false;

			AddStartUrl("http://a.com");

			Downloader = new MyDownloader();
			var process = new EntityProcessor<MyEntity>(new MyExtractor());
			AddPageProcessor(process);
		}


		[EntityTable("test", "MultiSupplement")]
		private class MyEntity : SpiderEntity
		{
			[PropertyDefine(Expression = "a")]
			public string a { get; set; }

			[PropertyDefine(Expression = "b")]
			public string b { get; set; }
		}
	}
}
