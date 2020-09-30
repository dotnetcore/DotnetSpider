using System;
using DotnetSpider.Downloader;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.Agent
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddAgent<TDownloader>(this IServiceCollection services,
			Action<AgentOptions> configure = null)
			where TDownloader : class, IDownloader
		{
			services.AddHttpClient();

			if (configure != null)
			{
				services.Configure(configure);
			}

			services.AddSingleton<IDownloader, TDownloader>();
			services.AddHostedService<AgentService>();
			return services;
		}
	}
}
