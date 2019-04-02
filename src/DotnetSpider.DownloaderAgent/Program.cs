using System;
using System.Linq;
using DotnetSpider.Downloader;
using DotnetSpider.Downloader.Internal;
using DotnetSpider.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotnetSpider.DownloaderAgent
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
						builder.AddDownloaderAgent(x =>
						{
							x.UseFileLocker();
							x.UseDefaultAdslRedialer();
							x.UseDefaultInternetDetector();
						});
						builder.AddSpiderStatisticsCenter(x => x.UseMemory());
					});
					services.AddHostedService<LocalDownloaderAgent>();
				})
				.UseEnvironment(args.Contains("/dev") ? EnvironmentName.Development : EnvironmentName.Production)
				.UseContentRoot(AppDomain.CurrentDomain.BaseDirectory)
				.Build();
			host.Run();
		}
	}
}