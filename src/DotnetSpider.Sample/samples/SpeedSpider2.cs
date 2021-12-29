using System;
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
	/// <summary>
	/// https://localhost:5001/WeatherForecast
	/// </summary>
	public class SpeedSpider2 : Spider
	{
		public static async Task RunAsync()
		{
			var builder = Builder.CreateDefaultBuilder<SpeedSpider2>(options =>
			{
				options.Speed = 100;
			});
			builder.UseSerilog();
			builder.UseDownloader<HttpClientDownloader>();
			builder.IgnoreServerCertificateError();
			builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
			await builder.Build().RunAsync();
		}

		public SpeedSpider2(IOptions<SpiderOptions> options,
			DependenceServices services,
			ILogger<Spider> logger) : base(
			options, services, logger)
		{
		}

		protected override async Task InitializeAsync(CancellationToken stoppingToken = default)
		{
			for (var i = 0; i < 100000; ++i)
			{
				await AddRequestsAsync(new Request("https://localhost:5001/WeatherForecast?_v=" + i));
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

			private DateTime _start;

			public override Task InitializeAsync()
			{
				_start = DateTime.Now;
				return Task.CompletedTask;
			}

			public override Task HandleAsync(DataFlowContext context)
			{
				Interlocked.Increment(ref _downloadCount);
				if ((_downloadCount % 100) == 0)
				{
					var sec = (DateTime.Now - _start).TotalSeconds;
					var speed = (decimal)(_downloadCount / sec);
					Logger.LogInformation($"Speed {decimal.Round(speed, 2)}");
				}

				return Task.CompletedTask;
			}
		}
	}
}
