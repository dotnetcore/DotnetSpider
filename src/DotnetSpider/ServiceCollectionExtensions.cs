using System;
using DotnetSpider.Core;
using DotnetSpider.Data;
using DotnetSpider.Downloader;
using DotnetSpider.Downloader.Internal;
using DotnetSpider.MessageQueue;
using DotnetSpider.Network;
using DotnetSpider.Network.InternetDetector;
using DotnetSpider.Statistics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace DotnetSpider
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection ConfigureAppConfiguration(this IServiceCollection services,
			string config = null,
			string[] args = null, bool loadCommandLine = true)
		{
			Check.NotNull(services, nameof(services));

			var configurationBuilder = Framework.CreateConfigurationBuilder(config, args, loadCommandLine);
			IConfigurationRoot configurationRoot = configurationBuilder.Build();
			services.AddSingleton<IConfiguration>(configurationRoot);

			return services;
		}
 
		public static IServiceCollection AddDownloadCenter(this IServiceCollection services,
			Action<DownloadCenterBuilder> configure = null)
		{
 
			services.AddSingleton<IDownloadCenter, DownloadCenter>();

			DownloadCenterBuilder downloadCenterBuilder = new DownloadCenterBuilder(services);
			configure?.Invoke(downloadCenterBuilder);

			return services;
		}

		public static IServiceCollection AddLocalDownloadCenter(this IServiceCollection services)
		{
			services.AddSingleton<IDownloadCenter, LocalDownloadCenter>();
			services.AddSingleton<IDownloaderAgentStore, LocalDownloaderAgentStore>();
			return services;
		}

		public static DownloadCenterBuilder UseMySqlDownloaderAgentStore(this DownloadCenterBuilder builder)
		{
			builder.Services.AddSingleton<IDownloaderAgentStore, MySqlDownloaderAgentStore>();
			return builder;
		}

		public static DownloadCenterBuilder UseMemoryDownloaderAgentStore(this DownloadCenterBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));
			builder.Services.AddSingleton<IDownloaderAgentStore, LocalDownloaderAgentStore>();
			return builder;
		}

		#region  Message queue

		public static IServiceCollection AddLocalMessageQueue(this IServiceCollection services)
		{
			services.AddSingleton<IMessageQueue, LocalMessageQueue>();
			return services;
		}

		#endregion

		#region DownloaderAgent

		public static IServiceCollection AddDownloaderAgent(this IServiceCollection services,
			Action<AgentBuilder> configure = null)
		{
			services.AddSingleton<IDownloaderAllocator, DownloaderAllocator>();
			services.AddSingleton<IDownloaderAgent, DefaultDownloaderAgent>();
			services.AddSingleton<NetworkCenter>();
			services.AddScoped<IDownloaderAgentOptions, DownloaderAgentOptions>();

			AgentBuilder spiderAgentBuilder = new AgentBuilder(services);
			configure?.Invoke(spiderAgentBuilder);

			return services;
		}

		public static IServiceCollection AddLocalDownloaderAgent(this IServiceCollection services,
			Action<AgentBuilder> configure = null)
		{
			services.AddSingleton<IDownloaderAllocator, DownloaderAllocator>();
			services.AddSingleton<IDownloaderAgent, LocalDownloaderAgent>();
			services.AddSingleton<NetworkCenter>();
			services.AddScoped<IDownloaderAgentOptions, DownloaderAgentOptions>();

			AgentBuilder spiderAgentBuilder = new AgentBuilder(services);
			configure?.Invoke(spiderAgentBuilder);

			return services;
		}

		public static AgentBuilder UseFileLocker(this AgentBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));

			builder.Services.AddSingleton<ILockerFactory, FileLockerFactory>();

			return builder;
		}

		public static AgentBuilder UseDefaultAdslRedialer(this AgentBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));

			builder.Services.AddSingleton<IAdslRedialer, DefaultAdslRedialer>();

			return builder;
		}

		public static AgentBuilder UseDefaultInternetDetector(this AgentBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));

			builder.Services.AddSingleton<IInternetDetector, DefaultInternetDetector>();

			return builder;
		}

		public static AgentBuilder UseVpsInternetDetector(this AgentBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));

			builder.Services.AddSingleton<IInternetDetector, VpsInternetDetector>();

			return builder;
		}

		#endregion

		#region  Statistics

		public static IServiceCollection AddSpiderStatisticsCenter(this IServiceCollection services,
			Action<StatisticsBuilder> configure)
		{
			services.AddSingleton<IStatisticsCenter, StatisticsCenter>();

			var spiderStatisticsBuilder = new StatisticsBuilder(services);
			configure?.Invoke(spiderStatisticsBuilder);

			return services;
		}

		public static StatisticsBuilder UseMemory(this StatisticsBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));
			builder.Services.AddSingleton<IStatisticsStore, MemoryStatisticsStore>();
			return builder;
		}

		public static StatisticsBuilder UseMySql(this StatisticsBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));
			builder.Services.AddSingleton<IStatisticsStore, MySqlStatisticsStore>();
			return builder;
		}

		#endregion
	}
}