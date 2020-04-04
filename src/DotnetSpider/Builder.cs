using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using DotnetSpider.Agent;
using DotnetSpider.AgentRegister;
using DotnetSpider.AgentRegister.Store;
using DotnetSpider.Extensions;
using DotnetSpider.Infrastructure;
using DotnetSpider.Proxy;
using DotnetSpider.Statistics;
using DotnetSpider.Statistics.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using SwiftMQ.DependencyInjection;

namespace DotnetSpider
{
	public class Builder : HostBuilder
	{
		private Builder()
		{
		}

		/// <summary>
		/// Create a spider builder only contains spider background service
		/// </summary>
		/// <param name="configureDelegate"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Builder CreateBuilder<T>(Action<SpiderOptions> configureDelegate = null)
			where T : Spider
		{
			return CreateBuilder(typeof(T), null, configureDelegate);
		}

		/// <summary>
		/// Create a spider builder only contains spider background service
		/// </summary>
		/// <param name="args"></param>
		/// <param name="configureDelegate"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Builder CreateBuilder<T>(string[] args, Action<SpiderOptions> configureDelegate = null)
			where T : Spider
		{
			return CreateBuilder(typeof(T), args, configureDelegate);
		}

		public static Builder CreateBuilder(Type type, string[] args = null,
			Action<SpiderOptions> configureDelegate = null)
		{
			var hostBuilder = CreateBuilder(args, configureDelegate);
			hostBuilder.ConfigureServices(services =>
			{
				services.AddSingleton(typeof(IHostedService), type);
			});
			return hostBuilder;
		}

		/// <summary>
		/// Create a default local spider builder
		/// </summary>
		/// <param name="configureDelegate"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Builder CreateDefaultBuilder<T>(Action<SpiderOptions> configureDelegate = null) where T : Spider
		{
			return CreateDefaultBuilder<T>(null, configureDelegate);
		}

		/// <summary>
		/// Create a default local spider builder
		/// </summary>
		/// <param name="args"></param>
		/// <param name="configureDelegate"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Builder CreateDefaultBuilder<T>(
			string[] args, Action<SpiderOptions> configureDelegate = null) where T : Spider
		{
			return CreateDefaultBuilder(typeof(T), args, configureDelegate);
		}

		public static Builder CreateDefaultBuilder(Type type,
			string[] args, Action<SpiderOptions> configureDelegate = null)
		{
			var hostBuilder = CreateBuilder(args, configureDelegate);
			hostBuilder.ConfigureServices(services =>
			{
				var configuration = hostBuilder.GetConfiguration();
				if (configuration != null)
				{
					services.Configure<AgentOptions>(configuration);
				}

				services.AddSwiftMQ();
				services.AddStatistics<MemoryStatisticsStore>();
				services.AddAgentRegister<MemoryAgentStore>();
				services.AddAgent();
				services.AddSingleton(typeof(IHostedService), type);
			});
			return hostBuilder;
		}

		private static Builder CreateBuilder(string[] args, Action<SpiderOptions> configureDelegate = null)
		{
			var hostBuilder = new Builder();
			hostBuilder.UseContentRoot(Directory.GetCurrentDirectory());
			hostBuilder.ConfigureHostConfiguration(config =>
			{
				config.AddEnvironmentVariables("DOTNET_");
				if (args == null)
				{
					return;
				}

				config.AddCommandLine(args);
			});
			hostBuilder.ConfigureAppConfiguration(
				(hostingContext, config) =>
				{
					var hostingEnvironment = hostingContext.HostingEnvironment;
					config.AddJsonFile("appsettings.json", true, true)
						.AddJsonFile("appsettings." + hostingEnvironment.EnvironmentName + ".json", true, true);
					if (hostingEnvironment.IsDevelopment() && !string.IsNullOrEmpty(hostingEnvironment.ApplicationName))
					{
						var assembly = Assembly.Load(new AssemblyName(hostingEnvironment.ApplicationName));
						config.AddUserSecrets(assembly, true);
					}

					config.AddEnvironmentVariables();

					var list = new List<string> {"--DOTNET_SPIDER_MODEL", "LOCAL"};
					if (args != null)
					{
						list.AddRange(args);
					}

					config.AddCommandLine(list.ToArray());
				}).ConfigureLogging((hostingContext, logging) =>
			{
				var num = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 1 : 0;
				if (num != 0)
				{
					logging.AddFilter<EventLogLoggerProvider>(
						level => level >= LogLevel.Warning);
				}

				logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
				logging.AddConsole();
				logging.AddDebug();
				logging.AddEventSourceLogger();
				if (num == 0)
				{
					return;
				}

				logging.AddEventLog();
			}).ConfigureServices(services =>
			{
				var configuration = hostBuilder.GetConfiguration();
				if (configuration != null)
				{
					services.Configure<SpiderOptions>(configuration);
				}

				if (configureDelegate != null)
				{
					services.Configure(configureDelegate);
				}

				services.AddSingleton<SpiderServices>();
				services.AddHostedService<ArgumentPrintService>();
				services.AddHostedService<ProxyService>();
				services.AddHttpClient();
				services.AddTransient<HttpMessageHandlerBuilder, ProxyHttpMessageHandlerBuilder>();
				services.AddSingleton<IProxyValidator, DefaultProxyValidator>();
				services.AddSingleton<IProxyPool, ProxyPool>();
				services.AddSingleton<IStatisticsClient, StatisticsClient>();
			}).UseDefaultServiceProvider((context, options) =>
			{
				var flag = context.HostingEnvironment.IsDevelopment();
				options.ValidateScopes = flag;
				options.ValidateOnBuild = flag;
			});
			return hostBuilder;
		}
	}
}
