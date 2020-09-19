using System;
using DotnetSpider.Scheduler;
using DotnetSpider.Statistics;
using Microsoft.Extensions.Hosting;
using IMessageQueue = DotnetSpider.MessageQueue.IMessageQueue;

namespace DotnetSpider
{
	public class DependenceServices : IDisposable
	{
		public IServiceProvider ServiceProvider { get; }
		public IScheduler Scheduler { get; }
		public IMessageQueue MessageQueue { get; }
		public IStatisticsClient StatisticsClient { get; }
		public IHostApplicationLifetime ApplicationLifetime { get; }

		public DependenceServices(IServiceProvider serviceProvider,
			IScheduler scheduler,
			IMessageQueue messageQueue,
			IStatisticsClient statisticsClient,
			IHostApplicationLifetime applicationLifetime)
		{
			ServiceProvider = serviceProvider;
			Scheduler = scheduler;
			MessageQueue = messageQueue;
			StatisticsClient = statisticsClient;
			ApplicationLifetime = applicationLifetime;
		}

		public void Dispose()
		{
			MessageQueue.Dispose();
			Scheduler.Dispose();
		}
	}
}
