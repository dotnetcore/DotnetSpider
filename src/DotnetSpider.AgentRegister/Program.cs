using System;
using System.Threading.Tasks;
using DotnetSpider.Agent;
using DotnetSpider.RabbitMQ;
using DotnetSpider.Statistics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.AgentRegister
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
				.WriteTo.Console().WriteTo.RollingFile("logs/agent-register.txt")
				.CreateLogger();

			var builder = Host.CreateDefaultBuilder(args);
			builder.ConfigureServices(x =>
			{
				x.AddHttpClient();
				x.AddAgentRegister();
				x.AddStatistics();
			});
			builder.UseRabbitMQ();
			builder.UseSerilog();
			await builder.Build().RunAsync();
			Environment.Exit(0);
		}
	}
}
