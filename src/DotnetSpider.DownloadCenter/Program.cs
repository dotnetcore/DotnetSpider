using System;
using System.Linq;
using DotnetSpider.Downloader.Internal;
using DotnetSpider.Kafka;
using DotnetSpider.Statistics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.DownloadCenter
{
	class Program
	{
		static void Main(string[] args)
		{
			var	configure = new LoggerConfiguration()
#if DEBUG
				.MinimumLevel.Verbose()
#else
				.MinimumLevel.Information()
#endif
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.Enrich.FromLogContext()
				.WriteTo.Console().WriteTo
				.RollingFile("dotnet-spider.log");
			Log.Logger = configure.CreateLogger();
			
			var host = new HostBuilder()
				.ConfigureLogging(x => { x.AddSerilog(); })
				.ConfigureAppConfiguration(x => x.AddJsonFile("appsettings.json"))
				.ConfigureServices((hostContext, services) =>
				{
					services.AddKafkaMessageQueue();
					services.AddDownloadCenter(x => x.UseMySqlDownloaderAgentStore());
					services.AddSpiderStatisticsCenter(x => x.UseMySql());
					services.AddHostedService<Downloader.DefaultDownloadAgentRegisterCenter>();
					services.AddHostedService<StatisticsCenter>();
				})
				.UseEnvironment(args.Contains("/dev") ? EnvironmentName.Development : EnvironmentName.Production)
				.UseContentRoot(AppDomain.CurrentDomain.BaseDirectory)
				.Build();
			host.Run();
		}
	}
}