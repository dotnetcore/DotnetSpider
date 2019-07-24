using System;
using DotnetSpider.Common;
using DotnetSpider.DataFlow;
using DotnetSpider.DownloadAgent;
using DotnetSpider.DownloadAgentRegisterCenter;
using DotnetSpider.DownloadAgentRegisterCenter.Internal;
using DotnetSpider.EventBus;
using DotnetSpider.Network;
using DotnetSpider.Network.InternetDetector;
using DotnetSpider.Statistics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotnetSpider
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection ConfigureAppConfiguration(this IServiceCollection services,
			string config = null)
		{
			Check.NotNull(services, nameof(services));

			var configurationBuilder = Framework.CreateConfigurationBuilder(config);
			IConfigurationRoot configurationRoot = configurationBuilder.Build();
			services.AddSingleton<IConfiguration>(configurationRoot);

			return services;
		}

		#region DownloadCenter

		public static IServiceCollection AddDownloadCenter(this IServiceCollection services,
			Action<DownloadAgentRegisterCenterBuilder> configure = null)
		{
			services.AddSingleton<IHostedService, DefaultDownloadAgentRegisterCenter>();

			DownloadAgentRegisterCenterBuilder downloadCenterBuilder = new DownloadAgentRegisterCenterBuilder(services);
			configure?.Invoke(downloadCenterBuilder);

			return services;
		}

		public static IServiceCollection AddLocalDownloadCenter(this IServiceCollection services)
		{
			services.AddDownloadCenter(x => x.UseLocalDownloaderAgentStore());
			return services;
		}

		public static DownloadAgentRegisterCenterBuilder UseMySqlDownloaderAgentStore(
			this DownloadAgentRegisterCenterBuilder builder)
		{
			builder.Services.AddSingleton<IDownloaderAgentStore, MySqlDownloaderAgentStore>();
			return builder;
		}

		public static DownloadAgentRegisterCenterBuilder UseLocalDownloaderAgentStore(
			this DownloadAgentRegisterCenterBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));
			builder.Services.AddSingleton<IDownloaderAgentStore, LocalDownloaderAgentStore>();
			return builder;
		}

		#endregion

		#region  EventBus

		public static IServiceCollection AddLocalEventBus(this IServiceCollection services)
		{
			services.AddSingleton<IEventBus, LocalEventBus>();
			return services;
		}

		#endregion

		#region DownloaderAgent

		public static IServiceCollection AddDownloaderAgent(this IServiceCollection services,
			Action<DownloaderAgentBuilder> configure = null)
		{
			services.AddSingleton<IHostedService, DefaultDownloaderAgent>();
			services.AddSingleton<NetworkCenter>();
			services.AddSingleton<DownloaderAgentOptions>();

			DownloaderAgentBuilder spiderAgentBuilder = new DownloaderAgentBuilder(services);
			configure?.Invoke(spiderAgentBuilder);

			return services;
		}

		public static DownloaderAgentBuilder UseFileLocker(this DownloaderAgentBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));

			builder.Services.AddSingleton<ILockerFactory, FileLockerFactory>();

			return builder;
		}

		public static DownloaderAgentBuilder UseDefaultAdslRedialer(this DownloaderAgentBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));

			builder.Services.AddSingleton<IAdslRedialer, DefaultAdslRedialer>();

			return builder;
		}

		public static DownloaderAgentBuilder UseDefaultInternetDetector(this DownloaderAgentBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));

			builder.Services.AddSingleton<IInternetDetector, DefaultInternetDetector>();

			return builder;
		}

		public static DownloaderAgentBuilder UseVpsInternetDetector(this DownloaderAgentBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));

			builder.Services.AddSingleton<IInternetDetector, VpsInternetDetector>();

			return builder;
		}

		#endregion

		#region  Statistics

		public static IServiceCollection AddStatisticsCenter(this IServiceCollection services,
			Action<StatisticsBuilder> configure)
		{
			services.AddSingleton<IHostedService, StatisticsCenter>();

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

		#region DotnetSpider

		public static IServiceCollection AddDotnetSpider(this IServiceCollection services)
		{
			return services;
		}

		#endregion
	}
}