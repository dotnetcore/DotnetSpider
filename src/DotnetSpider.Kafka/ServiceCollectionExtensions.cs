using DotnetSpider.DataFlow;
using DotnetSpider.MessageQueue;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.Kafka
{
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// 单机模式
		/// 在单机模式下，使用内存型消息队列，因此只有在此作用域 SpiderBuilder 下构建的的爬虫才会共用一个消息队列。
		/// </summary>
		/// <returns>爬虫构造器</returns>
		public static IServiceCollection AddKafka(this IServiceCollection services)
		{
			Check.NotNull(services, nameof(services));

			services.AddSingleton<KafkaOptions>();
			services.AddSingleton<IMq, KafkaMq>();
			return services;
		}
	}
}
