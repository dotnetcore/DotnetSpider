using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Downloader;
using DotnetSpider.Infrastructure;
using DotnetSpider.Scheduler.Component;
using DotnetSpider.Selector;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace DotnetSpider.Sample.samples
{
	public class WholeSiteSpider : Spider
	{
		public static async Task RunAsync()
		{
			var builder = Builder.CreateDefaultBuilder<WholeSiteSpider>(options =>
			{
				options.Depth = 1000;
			});
			builder.UseDownloader<HttpClientDownloader>();
			builder.UseSerilog();
			builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
			await builder.Build().RunAsync();
		}

		public WholeSiteSpider(IOptions<SpiderOptions> options,
			DependenceServices services,
			ILogger<Spider> logger) : base(
			options, services, logger)
		{
		}

		protected override async Task InitializeAsync(CancellationToken stoppingToken)
		{
			AddDataFlow(new MyDataParser());
			AddDataFlow(new ConsoleStorage()); // 控制台打印采集结果
			await AddRequestsAsync("http://www.cnblogs.com/"); // 设置起始链接
		}

		protected override (string Id, string Name) GetIdAndName()
		{
			return (ObjectId.NewId().ToString(), "博客园全站采集");
		}

		class MyDataParser : DataParser
		{
			public MyDataParser()
			{
				AddRequiredValidator("cnblogs\\.com");
				AddFollowRequestQuerier(Selectors.XPath("."));
			}

			protected override Task Parse(DataContext context)
			{
				context.AddData("URL", context.Request.Url);
				context.AddData("Title", context.Selectable.XPath(".//title")?.Value);
				return Task.CompletedTask;
			}
		}
	}
}
