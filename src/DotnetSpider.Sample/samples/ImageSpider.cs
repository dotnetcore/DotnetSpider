using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Http;
using DotnetSpider.Scheduler.Component;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace DotnetSpider.Sample.samples
{
	public class ImageSpider : Spider
	{
		public static async Task RunAsync()
		{
			var builder = Builder.CreateDefaultBuilder<ImageSpider>();
			builder.UseSerilog();
			builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
			await builder.Build().RunAsync();
		}

		public ImageSpider(IOptions<SpiderOptions> options,
			SpiderServices services,
			ILogger<Spider> logger) : base(options, services, logger)
		{
		}

		protected override async Task InitializeAsync(CancellationToken stoppingToken)
		{
			await AddRequestsAsync(
				new Request("https://www.cnblogs.com/images/logo_small.gif"));
			AddDataFlow(new ImageStorage());
		}
	}
}
