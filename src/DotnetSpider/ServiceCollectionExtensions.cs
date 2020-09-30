using System;
using DotnetSpider.Agent;
using DotnetSpider.Downloader;
using DotnetSpider.Infrastructure;
using DotnetSpider.Proxy;
using DotnetSpider.Scheduler;
using DotnetSpider.Scheduler.Component;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace DotnetSpider
{
	public static class ServiceCollectionExtensions
	{
		public static Builder UseQueueDfsScheduler(this Builder builder)
		{
			builder.ConfigureServices(x => { x.TryAddSingleton<IScheduler, QueueDfsScheduler>(); });
			return builder;
		}

		public static Builder UseQueueBfsScheduler(this Builder builder)
		{
			builder.ConfigureServices(x => { x.TryAddSingleton<IScheduler, QueueBfsScheduler>(); });
			return builder;
		}

		public static Builder UseQueueDistinctBfsScheduler<T>(this Builder builder)
			where T : class, IDuplicateRemover
		{
			builder.ConfigureServices(x =>
			{
				x.TryAddSingleton<IDuplicateRemover, T>();
				x.TryAddSingleton<IScheduler, QueueDistinctBfsScheduler>();
			});
			return builder;
		}

		// ReSharper disable once InconsistentNaming
		public static Builder UseMD5HashAlgorithmService(this Builder builder)
		{
			builder.ConfigureServices(x =>
			{
				x.AddSingleton<IHashAlgorithmService, MD5HashAlgorithmService>();
			});
			return builder;
		}

		public static Builder IgnoreServerCertificateError(this Builder builder)
		{
			builder.Properties["IGNORE_SSL_ERROR"] = "true";
			return builder;
		}

		/// <summary>
		/// 只有本地爬虫才能配置下载器，分布式爬虫的下载器注册是在下载器代理中
		/// </summary>
		/// <param name="builder"></param>
		/// <typeparam name="TDownloader"></typeparam>
		/// <returns></returns>
		public static Builder UseDownloader<TDownloader>(this Builder builder)
			where TDownloader : class, IDownloader
		{
			builder.Properties["DefaultDownloader"] = $"DOTNET_SPIDER_{typeof(TDownloader).Name}";

			builder.ConfigureServices(x =>
			{
				x.AddAgent<TDownloader>(opts =>
				{
					opts.AgentId = ObjectId.NewId().ToString();
					opts.AgentName = opts.AgentId;
				});
			});
			return builder;
		}

		public static Builder UseProxy<TProxySupplier>(this Builder builder)
			where TProxySupplier : class, IProxySupplier
		{
			builder.ConfigureServices(x =>
			{
				x.AddProxy<TProxySupplier>();
			});
			return builder;
		}

		public static IServiceCollection AddProxy<TProxySupplier>(this IServiceCollection serviceCollection)
			where TProxySupplier : class, IProxySupplier
		{
			serviceCollection.TryAddSingleton<IProxySupplier, TProxySupplier>();
			serviceCollection.TryAddSingleton<IProxyValidator, DefaultProxyValidator>();
			serviceCollection.TryAddSingleton<IProxyService, ProxyService>();
			serviceCollection.AddHostedService<ProxyBackgroundService>();
			return serviceCollection;
		}


		/// <summary>
		/// 使用 ADSL 拨号服务
		/// </summary>
		/// <param name="serviceCollection"></param>
		/// <param name="configure"></param>
		/// <returns></returns>
		public static IServiceCollection AddPPPoE(this IServiceCollection serviceCollection,
			Action<PPPoEOptions> configure)
		{
			serviceCollection.TryAddSingleton<PPPoEService>();
			if (configure != null)
			{
				serviceCollection.Configure(configure);
			}

			return serviceCollection;
		}

		public static IHostBuilder UseDockerLifetime(this IHostBuilder builder)
		{
			builder.ConfigureServices(x => { x.AddSingleton<IHostLifetime, DockerLifeTime>(); });
			return builder;
		}
	}
}
