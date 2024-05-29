using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DotnetSpider.Agent;
using DotnetSpider.AgentCenter;
using DotnetSpider.AgentCenter.Store;
using DotnetSpider.Infrastructure;
using DotnetSpider.MessageQueue;
using DotnetSpider.Scheduler;
using DotnetSpider.Scheduler.Component;
using DotnetSpider.Statistic;
using DotnetSpider.Statistic.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace DotnetSpider;

/// <summary>
/// 单机爬虫构建器
/// </summary>
public class Builder : HostBuilder
{
    private Builder()
    {
    }

    public static Builder CreateDefaultBuilder<T>(Action<SpiderOptions> configureDelegate = null)
        where T : Spider
    {
        return CreateDefaultBuilder<T>([], configureDelegate);
    }

    public static Builder CreateDefaultBuilder<T>(string[] args, Action<SpiderOptions> configureDelegate = null)
        where T : Spider
    {
        return CreateDefaultBuilder(typeof(T), args, configureDelegate);
    }

    public static Builder CreateDefaultBuilder(Type type, string[] args = null,
        Action<SpiderOptions> configure = null)
    {
        var builder = new Builder();
        ConfigureBuilder(builder, type, args, configure);
        builder.ConfigureServices(services =>
        {
            services.AddStatisticHostService<InMemoryStatisticStore>();
            services.AddAgentHostService();
            services.AddAgentCenterHostService<InMemoryAgentStore>();
        });
        return builder;
    }

    public static Builder CreateBuilder(Type type,
        string[] args = null, Action<SpiderOptions> configureDelegate = null)
    {
        var builder = new Builder();
        ConfigureBuilder(builder, type, args, configureDelegate);
        return builder;
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
        return CreateBuilder<T>(null, configureDelegate);
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

    private static void ConfigureBuilder(Builder builder, Type type, string[] args = null,
        Action<SpiderOptions> configureDelegate = null)
    {
        if (!type.IsAssignableTo(typeof(Spider)))
        {
            throw new ArgumentException($"Type {type.FullName} is not a spider");
        }

        builder.UseContentRoot(Directory.GetCurrentDirectory());
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddEnvironmentVariables("DOTNET_");
            if (args == null)
            {
                return;
            }

            config.AddCommandLine(args);
        });
        builder.ConfigureAppConfiguration(
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

                var list = new List<string> { "--DOTNET_SPIDER_MODEL", "LOCAL" };
                if (args != null)
                {
                    list.AddRange(args);
                }

                config.AddCommandLine(list.ToArray());
            }).ConfigureServices((context, services) =>
        {
            var configuration = context.Configuration;
            services.Configure<SpiderOptions>(configuration);

            if (configureDelegate != null)
            {
                services.Configure(configureDelegate);
            }

            services.AddHttpClient();
            services.AddHostedService<PrintArgumentService>();
            services.AddLocalMQ();
            services.TryAddSingleton<IStatisticService, StatisticService>();
            services.TryAddSingleton<DependenceServices>();
            services.TryAddSingleton<IRequestHasher, RequestHasher>();
            services.TryAddSingleton<IHashAlgorithmService, MurmurHashAlgorithmService>();
            services.AddSingleton(typeof(IHostedService), type);
        }).UseDefaultServiceProvider((context, options) =>
        {
            var flag = context.HostingEnvironment.IsDevelopment();
            options.ValidateScopes = flag;
            options.ValidateOnBuild = flag;
        });
        builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
    }
}
