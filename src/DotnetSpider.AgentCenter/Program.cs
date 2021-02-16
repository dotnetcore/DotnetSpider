using System;
using System.Threading.Tasks;
using DotnetSpider.MySql.AgentCenter;
using DotnetSpider.RabbitMQ;
using DotnetSpider.Statistics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.AgentCenter
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
				.WriteTo.Console().WriteTo.RollingFile("logs/agent-register.log")
				.CreateLogger();

			var builder = Host.CreateDefaultBuilder(args);
			builder.ConfigureServices((context, x) =>
			{
				x.Configure<AgentCenterOptions>(context.Configuration);
				x.AddHttpClient();
				x.AddAgentCenter<MySqlAgentStore>();
				x.AddStatistics<MySqlStatisticsStore>();
				x.AddRabbitMQ(context.Configuration);
			});
			builder.UseSerilog();
			await builder.Build().RunAsync();
			Environment.Exit(0);
		}
	}
}
