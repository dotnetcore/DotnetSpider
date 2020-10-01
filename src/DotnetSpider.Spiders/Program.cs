using System;
using System.Threading.Tasks;
using DotnetSpider.RabbitMQ;
using DotnetSpider.Scheduler;
using DotnetSpider.Scheduler.Component;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.Spiders
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
				.WriteTo.Console().WriteTo.RollingFile("logs/spiders.log")
				.CreateLogger();

			var typeName = Environment.GetEnvironmentVariable("DOTNET_SPIDER_TYPE");
			if (string.IsNullOrWhiteSpace(typeName))
			{
				Log.Logger.Error("Type name is missing");
				return;
			}
			else
			{
				var type = Type.GetType(typeName);
				if (type == null)
				{
					Log.Logger.Error("Type unfounded");
					return;
				}

				var builder = Builder.CreateBuilder(type);
				builder.UseSerilog();
				builder.UseRabbitMQ();
				builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
				await builder.Build().RunAsync();
			}


			Console.WriteLine("Bye!");
			Environment.Exit(0);
		}
	}
}
