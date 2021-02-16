using System;
using DotnetSpider.Scheduler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotnetSpider.MySql.Scheduler
{
	public static class ServiceCollectionExtensions
	{
		public static Builder UseMySqlQueueDfsScheduler(this Builder builder, Action<MySqlSchedulerOptions> configure)
		{
			builder.ConfigureServices((_, y) =>
			{
				y.Configure(configure);
				y.TryAddSingleton<IScheduler, MySqlQueueDfsScheduler>();
			});
			return builder;
		}


		public static Builder UseMySqlQueueBfsScheduler(this Builder builder, Action<MySqlSchedulerOptions> configure)
		{
			builder.ConfigureServices((x, y) =>
			{
				y.Configure(configure);
				y.TryAddSingleton<IScheduler, MySqlQueueBfsScheduler>();
			});
			return builder;
		}
	}
}
