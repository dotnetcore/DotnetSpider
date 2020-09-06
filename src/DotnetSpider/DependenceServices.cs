using System;
using DotnetSpider.Proxy;
using DotnetSpider.Scheduler;
using DotnetSpider.Statistics;
using Microsoft.Extensions.Hosting;
using IMessageQueue = DotnetSpider.MessageQueue.IMessageQueue;

namespace DotnetSpider
{
    public class DependenceServices
    {
        public IServiceProvider ServiceProvider { get; }
        public IScheduler Scheduler { get; }
        public IMessageQueue MessageQueue { get; }
        public IStatisticsClient StatisticsClient { get; }
        public IHostApplicationLifetime ApplicationLifetime { get; }
        public IProxyPool ProxyPool { get; }

        public bool IsDistributed { get; }

        public DependenceServices(IServiceProvider serviceProvider,
            IScheduler scheduler,
            IMessageQueue messageQueue,
            IStatisticsClient statisticsClient,
            IHostApplicationLifetime applicationLifetime,
            IProxyPool proxyPool)
        {
            ServiceProvider = serviceProvider;
            Scheduler = scheduler;
            MessageQueue = messageQueue;
            StatisticsClient = statisticsClient;
            ProxyPool = proxyPool;
            ApplicationLifetime = applicationLifetime;
            IsDistributed = !(messageQueue is MessageQueue.MessageQueue);
        }
    }
}
