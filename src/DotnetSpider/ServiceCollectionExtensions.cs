using DotnetSpider.Agent;
using DotnetSpider.Infrastructure;
using DotnetSpider.Proxy;
using DotnetSpider.Scheduler;
using DotnetSpider.Scheduler.Component;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotnetSpider
{
	public static class ServiceCollectionExtensions
	{
		public static Builder UseQueueBfsScheduler(this Builder builder)
		{
			builder.ConfigureServices(x => { x.AddSingleton<IScheduler, QueueBfsScheduler>(); });
			return builder;
		}

		public static Builder RegisterDownloader<TDownloader>(this Builder builder) where TDownloader : class, IDownloader
		{
			builder.ConfigureServices(x =>
			{
				x.AddSingleton<IDownloader, TDownloader>();
			});
			return builder;
		}

		public static Builder UseKuaidaili(this Builder builder)
		{
			builder.ConfigureServices(x => { x.AddSingleton<IProxySupplier, KuaidailiProxySupplier>(); });
			return builder;
		}

		public static Builder UseQueueDistinctBfsScheduler<T>(this Builder builder)
			where T : class, IDuplicateRemover
		{
			builder.ConfigureServices(x =>
			{
				x.AddSingleton<IDuplicateRemover, T>();
				x.AddSingleton<IScheduler, QueueDistinctBfsScheduler>();
			});
			return builder;
		}

		public static Builder UseQueueDistinctBfsScheduler(this Builder builder)
		{
			builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
			return builder;
		}

		public static Builder UseDockerLifetime(this Builder builder)
		{
			builder.ConfigureServices(x => { x.AddSingleton<IHostLifetime, DockerLifeTime>(); });
			return builder;
		}
	}
}
