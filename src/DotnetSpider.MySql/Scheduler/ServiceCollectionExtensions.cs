using System;
using DotnetSpider.Scheduler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotnetSpider.MySql.Scheduler;

public static class ServiceCollectionExtensions
{
    public static Builder UseMySqlQueueDfsScheduler(this Builder builder,
        Action<HostBuilderContext, MySqlSchedulerOptions> configure)
    {
        builder.ConfigureServices((x, y) =>
        {
            y.Configure(new Action<MySqlSchedulerOptions>(c =>
            {
                configure(x, c);
            }));
            y.AddSingleton<IScheduler, MySqlQueueDfsScheduler>();
        });
        return builder;
    }

    public static Builder UseMySqlQueueBfsScheduler(this Builder builder,
        Action<HostBuilderContext, MySqlSchedulerOptions> configure)
    {
        builder.ConfigureServices((x, y) =>
        {
            y.Configure(new Action<MySqlSchedulerOptions>(c =>
            {
                configure(x, c);
            }));
            y.AddSingleton<IScheduler, MySqlQueueBfsScheduler>();
        });
        return builder;
    }
}
