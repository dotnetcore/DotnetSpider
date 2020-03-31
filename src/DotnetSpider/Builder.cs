using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using DotnetSpider.Agent;
using DotnetSpider.AgentRegister;
using DotnetSpider.Infrastructure;
using DotnetSpider.Proxy;
using DotnetSpider.Statistics;
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

        public static Builder Create<T>(Action<SpiderOptions> configureDelegate) where T : Spider
        {
            return Create<T>(null, configureDelegate);
        }

        public static Builder Create<T>(
            string[] args, Action<SpiderOptions> configureDelegate = null) where T : Spider
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
                    IHostEnvironment hostingEnvironment = hostingContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile("appsettings." + hostingEnvironment.EnvironmentName + ".json", true, true);
                    if (hostingEnvironment.IsDevelopment() && !string.IsNullOrEmpty(hostingEnvironment.ApplicationName))
                    {
                        Assembly assembly = Assembly.Load(new AssemblyName(hostingEnvironment.ApplicationName));
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
                int num = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 1 : 0;
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
                var fields = typeof(HostBuilder).GetField("_appConfiguration",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (fields != null)
                {
                    var configuration = (IConfiguration) fields.GetValue(hostBuilder);
                    services.Configure<SpiderOptions>(configuration);
                    services.Configure<AgentOptions>(configuration);
                }

                if (configureDelegate != null)
                {
                    services.Configure(configureDelegate);
                }

                services.AddSingleton<SpiderServices>();
                services.AddSwiftMQ();
                services.AddHostedService<ArgumentPrintService>();
                services.AddStatistics();
                services.AddAgentRegister();
                services.AddAgent();
                services.AddHostedService<ProxyService>();
                services.AddHttpClient();
                services.AddTransient<HttpMessageHandlerBuilder, ProxyHttpMessageHandlerBuilder>();
                services.AddSingleton<IProxyValidator, DefaultProxyValidator>();
                services.AddSingleton<IProxyPool, ProxyPool>();
                services.AddSingleton<IStatisticsClient, StatisticsClient>();
                services.AddHostedService<T>();
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
