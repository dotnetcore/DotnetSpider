using System;
using System.Collections.Generic;
using System.IO;
using DotnetSpider.Core;
using DotnetSpider.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DotnetSpider.Spiders
{
	public class MyStartup : Startup
	{
		protected override List<string> DetectAssemblies()
		{
			return new List<string> { };
		}

		protected override void ConfigureService(IConfiguration configuration, SpiderHostBuilder builder)
		{
			builder.ConfigureLogging(b =>
			{
#if DEBUG
				b.SetMinimumLevel(LogLevel.Debug);
#else
                b.SetMinimumLevel(LogLevel.Information);
#endif
				b.AddSerilog();
			});

			var config = configuration["config"];
			builder.ConfigureAppConfiguration(b =>
			{
				if (!string.IsNullOrWhiteSpace(config) && File.Exists(config))
				{
					// 添加 JSON 配置文件
					b.AddJsonFile(config);
				}
				else
				{
					b.AddJsonFile("appsettings.json");
				}

				b.AddCommandLine(Environment.GetCommandLineArgs(), Framework.SwitchMappings);
				b.AddEnvironmentVariables();
			});

			var local = configuration["local"] == "true";

			builder.ConfigureServices(services =>
			{
				if (local)
				{
					services.AddLocalEventBus();
					services.AddLocalDownloadCenter();
					services.AddDownloaderAgent(x =>
					{
						x.UseFileLocker();
						x.UseDefaultAdslRedialer();
						x.UseDefaultInternetDetector();
					});
					services.AddStatisticsCenter(x =>
					{
						// 添加内存统计服务
						x.UseMemory();
					});
				}
				else
				{
					services.AddKafkaEventBus();
				}
			});
		}
	}
}