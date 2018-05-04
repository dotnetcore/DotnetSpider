using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Processor;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using DotnetSpider.Core.Infrastructure;
using System.Threading.Tasks;

namespace DotnetSpider.Sample
{
	[TaskName("CustomizeEntityExtractorWithEntityPipelineSpider")]
	public class CustomizeEntityExtractorWithEntityPipelineSpider : EntitySpider
	{
		private class MyDownloader : BaseDownloader
		{
			protected override Task<Page> DowloadContent(Request request, ISpider spider)
			{
				var page = new Page(request)
				{
					Content = "{}"
				};
				return Task.FromResult(page);
			}
		}

		private class MyExtractor : BaseEntityExtractor<MyEntity>
		{
			public override IEnumerable<MyEntity> Extract(Page page)
			{
				return new[] { new MyEntity { Name = "fuck", Age = 1 } };
			}
		}

		protected override void MyInit(params string[] arguments)
		{
			AddStartUrl("http://a.com");
			Downloader = new MyDownloader();
			var process = new EntityProcessor<MyEntity>(new MyExtractor());
			AddPageProcessors(process);
		}

		[EntityTable("test", "CustomizeEntityExtractorWithEntityPipeline")]
		private class MyEntity : SpiderEntity
		{
			[PropertyDefine(Expression = ".", Length = 100)]
			public string Name { get; set; }

			[PropertyDefine(Expression = ".")]
			public int Age { get; set; }
		}
	}
}
