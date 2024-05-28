using System;
using DotnetSpider.Scheduler;
using DotnetSpider.Statistic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using IMessageQueue = DotnetSpider.MessageQueue.IMessageQueue;

namespace DotnetSpider;

public class DependenceServices(
    IServiceProvider serviceProvider,
    IScheduler scheduler,
    IMessageQueue messageQueue,
    IStatisticService statisticService,
    IHostApplicationLifetime applicationLifetime,
    IConfiguration configuration,
    HostBuilderContext builderContext)
    : IDisposable
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
    public IScheduler Scheduler { get; } = scheduler;
    public IMessageQueue MessageQueue { get; } = messageQueue;
    public IStatisticService StatisticService { get; } = statisticService;
    public IHostApplicationLifetime ApplicationLifetime { get; } = applicationLifetime;
    public HostBuilderContext HostBuilderContext { get; } = builderContext;
    public IConfiguration Configuration { get; } = configuration;

    public void Dispose()
    {
        MessageQueue.Dispose();
        Scheduler.Dispose();
    }
}