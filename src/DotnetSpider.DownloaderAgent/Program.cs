using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotnetSpider.Common;
using DotnetSpider.DownloadAgent;
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
			var host = new HostBuilder().ConfigureAppConfiguration(x =>
				{
					if (File.Exists("appsettings.json"))
					{
						x.AddJsonFile("appsettings.json");
					}

					x.AddCommandLine(args);
					x.AddEnvironmentVariables();
				})
				.ConfigureLogging(x => { x.AddSerilog(); })
				.ConfigureServices((hostContext, services) =>
				{
					var options = new DownloaderAgentOptions(hostContext.Configuration);
					var logPath = $"/logs/{options.Name}.log";
					var configure = new LoggerConfiguration()
#if DEBUG
						.MinimumLevel.Verbose()
#else
						.MinimumLevel.Information()
#endif
						.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
						.Enrich.FromLogContext()
						.WriteTo.Console().WriteTo
						.RollingFile(logPath);
					Log.Logger = configure.CreateLogger();

					PrintEnvironment(hostContext.Configuration);
					
					services.AddSingleton<SpiderOptions>();
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
		
		private static void PrintEnvironment(IConfiguration configuration)
		{
			Framework.PrintInfo();
			foreach (var kv in configuration.GetChildren())
			{
				Log.Logger.Information($"运行参数   : {kv.Key} = {kv.Value}", 0, ConsoleColor.DarkYellow);
			}


			Log.Logger.Information($"运行目录   : {AppDomain.CurrentDomain.BaseDirectory}", 0,
				ConsoleColor.DarkYellow);
			Log.Logger.Information(
				$"操作系统   : {Environment.OSVersion} {(Environment.Is64BitOperatingSystem ? "X64" : "X86")}", 0,
				ConsoleColor.DarkYellow);
		}
	}
}