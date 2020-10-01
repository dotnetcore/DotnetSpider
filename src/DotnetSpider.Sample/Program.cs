using System;
using System.Threading;
using System.Threading.Tasks;
using Bert.RateLimiters;
using DotnetSpider.Http;
using DotnetSpider.Sample.samples;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.Sample
{
	class Program
	{
		static async Task Main(string[] args)
		{
			ThreadPool.SetMaxThreads(255, 255);
			ThreadPool.SetMinThreads(255, 255);

			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Information()
				.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.MinimumLevel.Override("System", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
				.Enrich.FromLogContext()
				.WriteTo.Console().WriteTo.RollingFile("logs/spider.log")
				.CreateLogger();

			// // await DistributedSpider.RunAsync();
			await EntitySpider.RunAsync();

			Console.WriteLine("Bye!");
			Environment.Exit(0);
		}

		private static async Task Test()
		{
			var Speed = 200;
			FixedTokenBucket limiter;

			/*
			 250:  1, 1, 4
			 10: 1,1, 100
			0.2: 1 秒中 0.2 个，则 5 秒 1个
			 */

			if (Speed >= 1)
			{
				var defaultTimeUnit = (int)(1000 / Speed);
				limiter = new FixedTokenBucket(1, 1, defaultTimeUnit);
			}
			else
			{
				var defaultTimeUnit = (int)((1 / Speed) * 1000);
				limiter = new FixedTokenBucket(1, 1, defaultTimeUnit);
			}

			var now = DateTime.Now;

			for (var i = 1; i < 100000000; ++i)
			{
				while (limiter.ShouldThrottle(1, out var waitTimeMillis))
				{
					await Task.Delay(waitTimeMillis);
				}

				if (i > 15 && (i % 10) == 0)
				{
					var seconds = (DateTime.Now - now).TotalSeconds;
					var speed = i / seconds;
					Console.WriteLine($"Speed: {speed}");
				}
			}
		}
	}
}
