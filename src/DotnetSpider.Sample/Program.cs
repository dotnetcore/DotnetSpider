using System;
using System.Threading;
using System.Threading.Tasks;
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
			await ImageSpider.RunAsync();

			Console.WriteLine("Bye!");
			Environment.Exit(0);
		}
	}
}
