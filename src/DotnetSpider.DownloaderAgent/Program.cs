using System;
using System.Linq;
using DotnetSpider.Common;
using DotnetSpider.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.DownloaderAgent
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
			
			var host = new HostBuilder().ConfigureAppConfiguration(x => x.AddJsonFile("appsettings.json"))
				.ConfigureLogging(x => { x.AddSerilog(); })
				.ConfigureServices((hostContext, services) =>
				{
					services.AddScoped<SpiderOptions>();
					services.AddKafkaEventBus();
					services.AddDownloaderAgent(x =>
					{
						x.UseFileLocker();
						x.UseDefaultAdslRedialer();
						x.UseDefaultInternetDetector();
					});
				})
				.UseEnvironment(args.Contains("/dev") ? EnvironmentName.Development : EnvironmentName.Production)
				.UseContentRoot(AppDomain.CurrentDomain.BaseDirectory)
				.Build();
			host.Run();
		}
	}
}