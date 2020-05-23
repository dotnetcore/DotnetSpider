using System;
using System.Threading.Tasks;
using DotnetSpider.Extensions;
using DotnetSpider.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.Agent
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
				.WriteTo.Console().WriteTo.RollingFile("logs/agent.log")
				.CreateLogger();

			var builder = Host.CreateDefaultBuilder(args);
			builder.ConfigureServices(x =>
			{
				var configuration = builder.GetConfiguration();
				if (configuration != null)
				{
					x.Configure<AgentOptions>(configuration);
					x.Configure<SpiderOptions>(configuration);
				}

				x.AddHttpClient();
				x.AddAgent();
			});
			builder.UseSerilog();
			builder.UseRabbitMQ();
			await builder.Build().RunAsync();
			Environment.Exit(0);
		}
	}
}
