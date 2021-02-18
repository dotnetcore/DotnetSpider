using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.Downloader;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using DotnetSpider.Scheduler;
using DotnetSpider.Scheduler.Component;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace DotnetSpider.Sample.samples
{
	public class SpeedSpider : Spider
	{
		public static async Task RunAsync()
		{
			var builder = Builder.CreateDefaultBuilder<SpeedSpider>(options =>
			{
				options.Speed = 100;
			});
			builder.UseSerilog();
			builder.UseDownloader<EmptyDownloader>();
			builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
			await builder.Build().RunAsync();
		}

		public SpeedSpider(IOptions<SpiderOptions> options,
			DependenceServices services,
			ILogger<Spider> logger) : base(
			options, services, logger)
		{
		}

		protected override async Task InitializeAsync(CancellationToken stoppingToken = default)
		{
			for (var i = 0; i < 100000; ++i)
			{
				await AddRequestsAsync(new Request("https://news.cnblogs.com/n/page/" + i)
				{
					Downloader = Downloaders.Empty
				});
			}

			AddDataFlow(new MyDataFlow());
		}

		protected override SpiderId GenerateSpiderId()
		{
			return new(ObjectId.CreateId().ToString(), "speed");
		}

		protected class MyDataFlow : DataFlowBase
		{
			private int _downloadCount;

			public override Task InitializeAsync()
			{
				return Task.CompletedTask;
			}

			public override Task HandleAsync(DataFlowContext context)
			{
				Interlocked.Increment(ref _downloadCount);
				if ((_downloadCount % 100) == 0)
				{
					Logger.LogInformation($"Complete {_downloadCount}");
				}

				return Task.CompletedTask;
			}
		}
	}
}
