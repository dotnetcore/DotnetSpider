using System;
using System.Threading.Tasks;
using DotnetSpider.Downloader;
using DotnetSpider.Extensions;
using DotnetSpider.RabbitMQ;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.Agent
{
	class Program
	{
		static void Main(string[] args)
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

			for (var i = 1; i <= 4; ++i)
			{
				var i2 = i;
				Task.Factory.StartNew(async () =>
				{
					try
					{
						var builder = Host.CreateDefaultBuilder(args);
						var id = i2;
						builder.UseSerilog();
						builder.ConfigureServices(x =>
						{
							x.AddAgent<HttpClientDownloader>(o =>
							{
								o.AgentId = "agent" + id;
								o.AgentName = o.AgentId;
							});
							var configuration = builder.GetConfiguration();
							x.AddRabbitMQ(configuration.GetSection("RabbitMQ"));
						});
						await builder.Build().RunAsync();
					}
					catch (Exception e)
					{
						Log.Logger.Fatal(e.ToString());
					}
				}, TaskCreationOptions.LongRunning).ConfigureAwait(false).GetAwaiter();
			}

			while (Console.ReadLine() == "exit")
			{
				break;
			}

			Environment.Exit(0);
		}
	}
}
