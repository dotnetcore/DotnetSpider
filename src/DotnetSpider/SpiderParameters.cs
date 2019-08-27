using System;
using DotnetSpider.Common;
using DotnetSpider.MessageQueue;
using DotnetSpider.Statistics;

namespace DotnetSpider
{
	public class SpiderParameters
	{
		internal IMq Mq { get; }

		internal IStatisticsService StatisticsService { get; }

		internal SpiderOptions SpiderOptions { get; }

		internal IServiceProvider ServiceProvider { get; }

		public SpiderParameters(IMq mq,
			IStatisticsService statisticsService,
			SpiderOptions options,
			IServiceProvider services)
		{
			Mq = mq;
			StatisticsService = statisticsService;
			SpiderOptions = options;
			ServiceProvider = services;
		}
	}
}
