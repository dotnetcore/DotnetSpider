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
		public static IServiceCollection AddSerilog(this IServiceCollection services,
			LoggerConfiguration configure = null)
		{
			Check.NotNull(services, nameof(services));

			if (configure == null)
			{
				configure = new LoggerConfiguration()
#if DEBUG
					.MinimumLevel.Verbose()
#else
				.MinimumLevel.Information()
#endif
					.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
					.Enrich.FromLogContext()
					.WriteTo.Console().WriteTo
					.RollingFile("dotnet-spider.log");
			}

			Log.Logger = configure.CreateLogger();

#if NETFRAMEWORK
			services.AddSingleton<ILoggerFactory, LoggerFactory>(provider =>
			{
				var loggerFactory = new LoggerFactory();
				loggerFactory.AddSerilog();
				return loggerFactory;
			});
			services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
#else
			services.AddLogging(b =>
			{
#if DEBUG
				b.SetMinimumLevel(LogLevel.Debug);
#else
                b.SetMinimumLevel(LogLevel.Information);
#endif
				b.AddSerilog();
			});
#endif
			return services;
		}


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

		public static IServiceCollection AddDotnetSpider(this IServiceCollection services,
			Action<DotnetSpiderBuilder> configureBuilder = null)
		{
			Check.NotNull(services, nameof(services));

			services.AddScoped<ISpiderOptions, SpiderOptions>();

			DotnetSpiderBuilder builder = new DotnetSpiderBuilder(services);
			configureBuilder?.Invoke(builder);

			return services;
		}

		public static DotnetSpiderBuilder AddDownloadCenter(this DotnetSpiderBuilder builder,
			Action<DownloadCenterBuilder> configure = null)
		{
			Check.NotNull(builder, nameof(builder));

			builder.Services.AddSingleton<IDownloadCenter, DownloadCenter>();

			DownloadCenterBuilder downloadCenterBuilder = new DownloadCenterBuilder(builder.Services);
			configure?.Invoke(downloadCenterBuilder);

			return builder;
		}

		public static DotnetSpiderBuilder AddLocalDownloadCenter(this DotnetSpiderBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));
			builder.Services.AddSingleton<IDownloadCenter, LocalDownloadCenter>();
			builder.Services.AddSingleton<IDownloaderAgentStore, LocalDownloaderAgentStore>();
			return builder;
		}

		public static DownloadCenterBuilder UseMySqlDownloaderAgentStore(this DownloadCenterBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));
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

		public static DotnetSpiderBuilder UseLocalMessageQueue(this DotnetSpiderBuilder builder)
		{
			builder.Services.AddSingleton<IMessageQueue, LocalMessageQueue>();
			return builder;
		}

		#endregion

		#region DownloaderAgent

		public static DotnetSpiderBuilder AddDownloaderAgent(this DotnetSpiderBuilder builder,
			Action<AgentBuilder> configure = null)
		{
			Check.NotNull(builder, nameof(builder));

			builder.Services.AddSingleton<IDownloaderAllocator, DownloaderAllocator>();
			builder.Services.AddSingleton<IDownloaderAgent, DefaultDownloaderAgent>();
			builder.Services.AddSingleton<NetworkCenter>();
			builder.Services.AddScoped<IDownloaderAgentOptions, DownloaderAgentOptions>();

			AgentBuilder spiderAgentBuilder = new AgentBuilder(builder.Services);
			configure?.Invoke(spiderAgentBuilder);

			return builder;
		}

		public static DotnetSpiderBuilder AddLocalDownloaderAgent(this DotnetSpiderBuilder builder,
			Action<AgentBuilder> configure = null)
		{
			Check.NotNull(builder, nameof(builder));

			builder.Services.AddSingleton<IDownloaderAllocator, DownloaderAllocator>();
			builder.Services.AddSingleton<IDownloaderAgent, LocalDownloaderAgent>();
			builder.Services.AddSingleton<NetworkCenter>();
			builder.Services.AddScoped<IDownloaderAgentOptions, DownloaderAgentOptions>();

			AgentBuilder spiderAgentBuilder = new AgentBuilder(builder.Services);
			configure?.Invoke(spiderAgentBuilder);

			return builder;
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

		public static DotnetSpiderBuilder AddSpiderStatisticsCenter(this DotnetSpiderBuilder builder,
			Action<StatisticsBuilder> configure)
		{
			Check.NotNull(builder, nameof(builder));

			builder.Services.AddSingleton<IStatisticsCenter, StatisticsCenter>();

			var spiderStatisticsBuilder = new StatisticsBuilder(builder.Services);
			configure?.Invoke(spiderStatisticsBuilder);

			return builder;
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