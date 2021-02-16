using DotnetSpider.Scheduler.Component;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace DotnetSpider.Scheduler
{
	public static class ServiceCollectionExtensions
	{
		public static Builder UseQueueDfsScheduler(this Builder builder)
		{
			builder.ConfigureServices(x =>
			{
				x.TryAddSingleton<IScheduler, QueueDfsScheduler>();
			});
			return builder;
		}

		public static Builder UseQueueBfsScheduler(this Builder builder)
		{
			builder.ConfigureServices(x => { x.TryAddSingleton<IScheduler, QueueBfsScheduler>(); });
			return builder;
		}

		public static Builder UseQueueDistinctBfsScheduler<TDuplicateRemover>(this Builder builder)
			where TDuplicateRemover : class, IDuplicateRemover
		{
			builder.ConfigureServices(x =>
			{
				x.TryAddSingleton<IDuplicateRemover, TDuplicateRemover>();
				x.TryAddSingleton<IScheduler, QueueDistinctBfsScheduler>();
			});
			return builder;
		}
	}
}
