using System;
using DotnetSpider.Downloader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

		// public static IHostBuilder UseAgent<TDownloader>(this IHostBuilder builder)
		// 	where TDownloader : class, IDownloader
		// {
		// 	builder.ConfigureServices(x =>
		// 	{
		// 		var configuration = builder.GetConfiguration();
		// 		if (configuration != null)
		// 		{
		// 			x.Configure<AgentOptions>(configuration);
		// 		}
		//
		// 		x.AddAgent<TDownloader>();
		// 	});
		// 	return builder;
		// }
		//
		// public static IHostBuilder UseAgent<TDownloader>(this IHostBuilder builder, Action<AgentOptions> configure)
		// 	where TDownloader : class, IDownloader
		// {
		// 	builder.ConfigureServices(x =>
		// 	{
		// 		x.AddAgent<TDownloader>(configure);
		// 	});
		// 	return builder;
		// }
	}
}
