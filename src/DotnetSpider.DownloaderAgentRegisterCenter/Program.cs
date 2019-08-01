using System;
using System.IO;
using System.Linq;
using DotnetSpider.Common;
using DotnetSpider.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.DownloaderAgentRegisterCenter
{
	class Program
	{
		static void Main(string[] args)
		{
			var host = new HostBuilder()
				.ConfigureLogging(x => { x.AddSerilog(); })
				.ConfigureAppConfiguration(x =>
				{
					if (File.Exists("appsettings.json"))
					{
						x.AddJsonFile("appsettings.json");
					}
					x.AddEnvironmentVariables(prefix: "DOTNET_SPIDER_");
				})
				.ConfigureServices((hostContext, services) =>
				{
					var configure = new LoggerConfiguration()
#if DEBUG
						.MinimumLevel.Verbose()
#else
						.MinimumLevel.Information()
#endif
						.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
						.Enrich.FromLogContext()
						.WriteTo.Console().WriteTo
						.RollingFile("/logs/register-center/register-center.log");
					Log.Logger = configure.CreateLogger();
					
					services.AddSingleton<SpiderOptions>();
					services.AddKafkaEventBus();
					services.AddDownloadCenter(x => x.UseMySqlDownloaderAgentStore());
					services.AddStatisticsCenter(x => x.UseMySql());
				})
				.UseEnvironment(args.Contains("/dev") ? EnvironmentName.Development : EnvironmentName.Production)
				.UseContentRoot(AppDomain.CurrentDomain.BaseDirectory)
				.Build();
			host.Run();
		}
	}
}