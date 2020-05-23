using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Parser.Formatters;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.HBase;
using DotnetSpider.Http;
using DotnetSpider.RabbitMQ;
using DotnetSpider.Sample.samples;
using DotnetSpider.Scheduler.Component;
using DotnetSpider.Selector;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.Sample
{
	class Program
	{
		static async Task Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Information()
				.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.MinimumLevel.Override("System", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
				.Enrich.FromLogContext()
				.WriteTo.Console().WriteTo.RollingFile("logs/spider.txt")
				.CreateLogger();

			await DistributedSpider.RunAsync();

			Console.WriteLine("Bye!");
			Environment.Exit(0);
		}
	}
}
