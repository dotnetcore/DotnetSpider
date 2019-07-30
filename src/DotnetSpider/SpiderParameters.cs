using System;
using DotnetSpider.Common;
using DotnetSpider.EventBus;
using DotnetSpider.Statistics;

namespace DotnetSpider
{
	public class SpiderParameters
	{
		internal IEventBus EventBus { get; }

		internal IStatisticsService StatisticsService { get; }

		internal SpiderOptions SpiderOptions { get; }

		internal IServiceProvider ServiceProvider { get; }

		public SpiderParameters(IEventBus eventBus,
			IStatisticsService statisticsService,
			SpiderOptions options,
			IServiceProvider services)
		{
			EventBus = eventBus;
			StatisticsService = statisticsService;
			SpiderOptions = options;
			ServiceProvider = services;
		}
	}
}
