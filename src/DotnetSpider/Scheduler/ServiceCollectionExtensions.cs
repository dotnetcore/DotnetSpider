using DotnetSpider.Scheduler.Component;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotnetSpider.Scheduler;

public static class ServiceCollectionExtensions
{
    public static Builder UseQueueDfsScheduler(this Builder builder)
    {
        builder.ConfigureServices(x =>
        {
            x.AddSingleton<IScheduler, QueueDfsScheduler>();
        });
        return builder;
    }

    public static Builder UseQueueBfsScheduler(this Builder builder)
    {
        builder.ConfigureServices(x => { x.AddSingleton<IScheduler, QueueBfsScheduler>(); });
        return builder;
    }

    public static Builder UseQueueDistinctBfsScheduler<TDuplicateRemover>(this Builder builder)
        where TDuplicateRemover : class, IDuplicateRemover
    {
        builder.ConfigureServices(x =>
        {
            x.AddSingleton<IDuplicateRemover, TDuplicateRemover>();
            x.AddSingleton<IScheduler, QueueDistinctBfsScheduler>();
        });
        return builder;
    }
}
