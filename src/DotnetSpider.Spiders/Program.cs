using System;
using System.IO;
using DotnetSpider.Core;
using DotnetSpider.Kafka;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.Spiders
{
	public class Program
	{
		static void Main(string[] args)
		{
			try
			{
				var builder = new SpiderHostBuilder();

				var configurationBuilder = Framework.CreateConfigurationBuilder(null, args);
				var configuration = configurationBuilder.Build();
				var @class = configuration["dotnetspider.spider.class"];
				var spiderId = configuration["dotnetspider.spider.id"];

				@class = "DotnetSpider.Spiders.CnblogsSpider";
				spiderId = "xxxxxxxx";


				var folder = Directory.Exists("/logs/") ? "/logs/" : "";

				var logPath = string.IsNullOrWhiteSpace(spiderId)
					? $"{folder}{DateTime.Now:yyyy-MM-dd HH:mm:ss}.log"
					: $"{folder}{spiderId}.log";

				var loggerConfiguration = new LoggerConfiguration()
					.MinimumLevel.Information()
					.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
					.Enrich.FromLogContext()
					.WriteTo.Console().WriteTo
					.RollingFile(logPath);
				Log.Logger = loggerConfiguration.CreateLogger();
				
				var spiderName = configuration["dotnetspider.spider.name"];
				if (string.IsNullOrWhiteSpace(@class) ||
				    string.IsNullOrWhiteSpace(spiderId) ||
				    string.IsNullOrWhiteSpace(spiderName)
				)
				{
					Log.Logger.Error($"执行爬虫的参数不正确: class {@class}, id {spiderId}, name {spiderName}");
					return;
				}

				var type = Type.GetType(@class);
				if (type == null)
				{
					Log.Logger.Error($"未找到爬虫类型: {@class}");
					return;
				}

				Log.Logger.Information($"获取爬虫类型 {type.FullName} 成功");
				builder.ConfigureAppConfiguration(x =>
				{
					x.AddCommandLine(args);
				});
				builder.ConfigureLogging(x =>
				{
					x.AddSerilog();
				});
				builder.ConfigureServices(services =>
				{
					services.AddKafkaEventBus();
				});
				builder.Register(type);
				var provider = builder.Build();

				var spider = provider.Create(type);
				Log.Logger.Information($"创建爬虫实例成功");
				spider.Id = spiderId;
				spider.Name = spiderName;

				Log.Logger.Information($"尝试启动爬虫实例");
				spider.Run();

				Log.Logger.Information($"爬虫实例退出");
			}
			catch (Exception e)
			{
				Log.Logger.Error($"执行失败: {e}");
			}
		}
	}
}