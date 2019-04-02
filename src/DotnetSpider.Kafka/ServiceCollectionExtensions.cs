using DotnetSpider.Data;
using DotnetSpider.MessageQueue;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.Kafka
{
	public static class ServiceCollectionExtensions
	{
		public static DotnetSpiderBuilder UserKafka(this DotnetSpiderBuilder builder)
		{
			Check.NotNull(builder,nameof(builder));
			builder.Services.AddSingleton<IMessageQueue, KafkaMessageQueue>();
			return builder;
		}
		
		/// <summary>
		/// 单机模式
		/// 在单机模式下，使用内存型消息队列，因此只有在此作用域 SpiderBuilder 下构建的的爬虫才会共用一个消息队列。
		/// </summary>
		/// <param name="builder">爬虫构造器</param>
		/// <returns>爬虫构造器</returns>
		public static SpiderBuilder UserKafka(this SpiderBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));

			builder.Services.AddSingleton<IMessageQueue, KafkaMessageQueue>();
			return builder;
		}
	}
}