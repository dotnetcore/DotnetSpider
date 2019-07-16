using System;
using System.Collections.Generic;
using System.IO;
using DotnetSpider.Common;
using DotnetSpider.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DotnetSpider.Spiders.Startup
{
	public class MyStartup : DotnetSpider.Startup
	{
		protected override List<string> DetectAssemblies()
		{
			return new List<string> {"DotnetSpider.Spiders"};
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
			builder.ConfigureAppConfiguration(x =>
			{
				if (!string.IsNullOrWhiteSpace(config) && File.Exists(config))
				{
					// 添加 JSON 配置文件
					x.AddJsonFile(config);
				}
				else
				{
					x.AddJsonFile("appsettings.json");
				}

				x.AddEnvironmentVariables(prefix: "DOTNET_SPIDER_");
				x.AddCommandLine(Environment.GetCommandLineArgs(), Framework.SwitchMappings);
			});

			var distributed = configuration["distributed"] == "true";

			builder.ConfigureServices(services =>
			{
				if (!distributed)
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