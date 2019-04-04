using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.MessageQueue;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;

namespace DotnetSpider.Kafka
{
	/// <summary>
	/// TODO: 确定 Kafka 这个客户端在网络断开、Kafka 崩溃情况下的表现，是否一直等待还是抛异常退出
	/// TODO: 如果会退出，则需要重试
	/// </summary>
	public class KafkaMessageQueue : IMessageQueue
	{
		private readonly Dictionary<string, Consumer<Null, string>> _consumers =
			new Dictionary<string, Consumer<Null, string>>();

		private readonly ILogger _logger;
		private readonly IProducer<Null, string> _producer;
		private readonly ConsumerConfig _config;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="options">爬虫选项</param>
		/// <param name="logger">日志接口</param>
		public KafkaMessageQueue(ISpiderOptions options,
			ILogger<KafkaMessageQueue> logger)
		{
			_logger = logger;
			_config = new ConsumerConfig
			{
				GroupId = options.KafkaConsumerGroup,
				BootstrapServers = options.KafkaBootstrapServers,
				// Note: The AutoOffsetReset property determines the start offset in the event
				// there are not yet any committed offsets for the consumer group for the
				// topic/partitions of interest. By default, offsets are committed
				// automatically, so in this example, consumption will only start from the
				// earliest message in the topic 'my-topic' the first time you run the program.
				AutoOffsetReset = AutoOffsetReset.Earliest
			};
			var productConfig = new ProducerConfig {BootstrapServers = options.KafkaBootstrapServers};
			_producer = new ProducerBuilder<Null, string>(productConfig).Build();
		}

		public async Task PublishAsync(string topic, params string[] messages)
		{
			if (messages == null || messages.Length == 0)
			{
#if DEBUG
				var stackTrace = new StackTrace();
				_logger?.LogDebug($"推送空消息到 Topic {topic}: {stackTrace}");
#endif
				return;
			}

			foreach (var message in messages)
			{
				await _producer.ProduceAsync(topic, new Message<Null, string> {Value = message});
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Subscribe(string topic, Action<string> action)
		{
			Unsubscribe(topic);

			Task.Factory.StartNew(() =>
			{
				using (var c = new ConsumerBuilder<Ignore, string>(_config).Build())
				{
					c.Subscribe(topic);
					try
					{
						while (true)
						{
							string msg = null;
							try
							{
								msg = c.Consume().Value;
								action(msg);
							}
							catch (ConsumeException e)
							{
								_logger?.LogError($"Kafka 接收消息 Topic {topic} 异常: {e.Error.Reason}");
							}
							catch (Exception e)
							{
								_logger?.LogError($"Topic {topic} 消费消息 {msg} 失败: {e}");
							}
						}
					}
					catch (OperationCanceledException oce)
					{
						// Ensure the consumer leaves the group cleanly and final offsets are committed.
						c.Close();
						_logger?.LogDebug($"Kafka 停止接收消息 Topic {topic}: {oce}");
					}
				}
			}).ConfigureAwait(false).GetAwaiter();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Unsubscribe(string topic)
		{
			if (_consumers.ContainsKey(topic))
			{
				_consumers[topic].Unsubscribe();
				_consumers[topic] = null;
			}
		}
	}
}