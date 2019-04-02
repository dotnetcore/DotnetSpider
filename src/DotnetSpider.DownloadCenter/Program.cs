using System;
using System.Linq;
using DotnetSpider.Downloader.Internal;
using DotnetSpider.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotnetSpider.DownloadCenter
{
	class Program
	{
		static void Main(string[] args)
		{
			var host = new HostBuilder().ConfigureAppConfiguration(x => x.AddJsonFile("appsettings.json"))
				.ConfigureServices((hostContext, services) =>
				{
					services.AddSerilog();
					services.AddDotnetSpider(builder =>
					{
						builder.UserKafka();
						builder.AddDownloadCenter(x => x.UseMySqlDownloaderAgentStore());
						builder.AddSpiderStatisticsCenter(x => x.UseMemory());
					});
					services.AddHostedService<LocalDownloadCenter>();
				})
				.UseEnvironment(args.Contains("/dev") ? EnvironmentName.Development : EnvironmentName.Production)
				.UseContentRoot(AppDomain.CurrentDomain.BaseDirectory)
				.Build();
			host.Run();
		}
	}
}