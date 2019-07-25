using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.Tests
{
	public class TestBase
	{
		protected TestBase()
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
		}
		
		protected readonly Lazy<SpiderProvider> SpiderProvider = new Lazy<SpiderProvider>(() =>
		{
			var builder = new SpiderHostBuilder()
				.ConfigureLogging(x => x.AddSerilog())
				.ConfigureAppConfiguration(x => x.AddJsonFile("appsettings.json"))
				.ConfigureServices(services =>
				{
					services.AddLocalEventBus();
					services.AddLocalDownloadCenter();
					services.AddDownloaderAgent(x =>
					{
						x.UseFileLocker();
						x.UseDefaultAdslRedialer();
						x.UseDefaultInternetDetector();
					});
					services.AddStatisticsCenter(x => x.UseMemory());
				});
			return builder.Build();
		});

		protected bool IsCI()
		{
			return Directory.Exists("/home/vsts/work");
		}

		protected ILogger<T> CreateLogger<T>()
		{
			var logger = new LoggerFactory();
			logger.AddSerilog();
			return logger.CreateLogger<T>();
		}
	}
}