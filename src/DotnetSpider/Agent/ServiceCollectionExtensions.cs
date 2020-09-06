using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotnetSpider.Agent
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddAgent(this IServiceCollection services)
		{
			services.TryAddSingleton<DownloaderFactory>();
			services.AddSingleton<IDownloader, HttpClientDownloader>();
			services.AddSingleton<IDownloader, PuppeteerDownloader>();
			services.AddSingleton<IDownloader, FileDownloader>();
			services.TryAddSingleton<PPPoEService>();
			services.AddHostedService<AgentService>();
			return services;
		}
	}
}
