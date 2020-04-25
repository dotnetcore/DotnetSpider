using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.Agent
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddAgent(this IServiceCollection services)
		{
			services.AddSingleton<DownloaderFactory>();
			services.AddSingleton<IDownloader, HttpClientDownloader>();
			services.AddSingleton<IDownloader, PuppeteerDownloader>();
			services.AddSingleton<IDownloader, FileDownloader>();
			services.AddSingleton<PPPoEService>();
			services.AddHostedService<AgentService>();
			return services;
		}
	}
}
