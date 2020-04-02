using System;
using DotnetSpider.Proxy;
using DotnetSpider.Scheduler;
using DotnetSpider.Statistics;
using Microsoft.Extensions.Hosting;
using SwiftMQ;

namespace DotnetSpider
{
    public class SpiderServices
    {
        public IServiceProvider ServiceProvider { get; }
        public IScheduler Scheduler { get; }
        public IMessageQueue MessageQueue { get; }
        public IStatisticsClient StatisticsClient { get; }
        public IHostApplicationLifetime ApplicationLifetime { get; }
        public IProxyPool ProxyPool { get; }

        public bool IsDistributed { get; }

        public SpiderServices(IServiceProvider serviceProvider,
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
            ServiceProvider = serviceProvider;
            ApplicationLifetime = applicationLifetime;
            IsDistributed = !(messageQueue is MessageQueue);
        }
    }
}