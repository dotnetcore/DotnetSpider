using System;
using DotnetSpider.Scheduler;
using DotnetSpider.Statistics;
using Microsoft.Extensions.Configuration;
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
		public HostBuilderContext HostBuilderContext { get; }
		public IConfiguration Configuration { get; }

		public DependenceServices(IServiceProvider serviceProvider,
			IScheduler scheduler,
			IMessageQueue messageQueue,
			IStatisticsClient statisticsClient,
			IHostApplicationLifetime applicationLifetime,
			IConfiguration configuration,
			HostBuilderContext builderContext)
		{
			ServiceProvider = serviceProvider;
			Scheduler = scheduler;
			MessageQueue = messageQueue;
			StatisticsClient = statisticsClient;
			ApplicationLifetime = applicationLifetime;
			HostBuilderContext = builderContext;
			Configuration = configuration;
		}

		public void Dispose()
		{
			MessageQueue.Dispose();
			Scheduler.Dispose();
		}
	}
}
