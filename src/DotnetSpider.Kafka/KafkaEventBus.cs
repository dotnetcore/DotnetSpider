using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using DotnetSpider.Common;
using DotnetSpider.EventBus;
using Microsoft.Extensions.Logging;
using Partitioner = Confluent.Kafka.Partitioner;

namespace DotnetSpider.Kafka
{
	/// <summary>
	/// TODO: 确定 Kafka 这个客户端在网络断开、Kafka 崩溃情况下的表现，是否一直等待还是抛异常退出
	/// TODO: 如果会退出，则需要重试
	/// </summary>
	public class KafkaEventBus : IEventBus
	{
		private readonly ConcurrentDictionary<string, IConsumer<Null, Event>> _consumers =
			new ConcurrentDictionary<string, IConsumer<Null, Event>>();

		private readonly ILogger _logger;
		private readonly IProducer<Null, Event> _producer;
		private readonly KafkaOptions _options;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="options">爬虫选项</param>
		/// <param name="logger">日志接口</param>
		public KafkaEventBus(KafkaOptions options,
			ILogger<KafkaEventBus> logger)
		{
			_logger = logger;
			_options = options;
			var productConfig = new ProducerConfig
			{
				Partitioner = Partitioner.ConsistentRandom,
				CompressionType = CompressionType.Lz4
			};
			SetClientConfig(productConfig);
			var builder =
				new ProducerBuilder<Null, Event>(productConfig).SetValueSerializer(new ProtobufSerializer<Event>());

			_producer = builder.Build();
		}

		public async Task PublishAsync(string topic, Event message)
		{
			if (message == null)
			{
#if DEBUG
				var stackTrace = new StackTrace();
				_logger.LogDebug($"Publish empty message to topic {topic}: {stackTrace}");
#endif
				return;
			}

			message.Timestamp = (long) DateTimeHelper.GetCurrentUnixTimeNumber();
			await _producer.ProduceAsync(topic,
				new Message<Null, Event>
				{
					Value = message
				});
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Subscribe(string topic, Action<Event> action)
		{
			if (_consumers.ContainsKey(topic))
			{
				_logger?.LogError($"Already subscribe {topic}");
				return;
			}

			if (_options.PartitionTopics.Contains(topic))
			{
				var adminClientConfig = new AdminClientConfig();
				SetClientConfig(adminClientConfig);
				using (var adminClient = new AdminClientBuilder(adminClientConfig).Build())
				{
					PrepareTopic(adminClient, topic);
				}
			}

			var config = new ConsumerConfig
			{
				GroupId = _options.ConsumerGroup,
				// Note: The AutoOffsetReset property determines the start offset in the event
				// there are not yet any committed offsets for the consumer group for the
				// topic/partitions of interest. By default, offsets are committed
				// automatically, so in this example, consumption will only start from the
				// earliest message in the topic 'my-topic' the first time you run the program.
				AutoOffsetReset = AutoOffsetReset.Earliest
			};
			SetClientConfig(config);
			var consumer = new ConsumerBuilder<Null, Event>(config)
				.SetValueDeserializer(new ProtobufDeserializer<Event>()).Build();
			consumer.Subscribe(topic);
			_consumers.TryAdd(topic, consumer);
			Task.Factory.StartNew(() =>
			{
				_logger.LogInformation("Subscribe: " + topic);
				while (_consumers.ContainsKey(topic))
				{
					Event msg = null;
					try
					{
						msg = consumer.Consume().Value;
					}
					catch (Exception e)
					{
						_logger?.LogError($"Consume kafka message failed on topic {topic}: {e}");
					}

					if (msg != null)
					{
						try
						{
							action(msg);
						}
						catch (Exception e)
						{
							_logger?.LogError($"Handle kafka message failed on topic {topic}: {e}");
						}
					}
					else
					{
						_logger?.LogWarning($"Ignore empty kafka message on topic {topic}");
					}
				}

				_logger?.LogWarning($"Exit consume kafka topic {topic}");
			}).ConfigureAwait(false).GetAwaiter();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Unsubscribe(string topic)
		{
			if (_consumers.ContainsKey(topic))
			{
				_consumers[topic].Unsubscribe();
				_consumers[topic].Dispose();
				_consumers.TryRemove(topic, out _);
			}
		}

		private void PrepareTopic(IAdminClient adminClient, string topic)
		{
			var metadata = adminClient.GetMetadata(topic, TimeSpan.FromSeconds(20));
			var partitionCount = metadata.Topics.First().Partitions.Count;
			if (partitionCount == 0)
			{
				adminClient.CreateTopicsAsync(new[]
				{
					new TopicSpecification
						{NumPartitions = _options.TopicPartitionCount, Name = topic, ReplicationFactor = 1}
				}).GetAwaiter().GetResult();
			}
			else
			{
				if (partitionCount < _options.TopicPartitionCount)
				{
					adminClient.CreatePartitionsAsync(new[]
					{
						new PartitionsSpecification
							{Topic = topic, IncreaseTo = _options.TopicPartitionCount}
					}).GetAwaiter().GetResult();
				}
			}
		}

		private void SetClientConfig(ClientConfig config)
		{
			config.BootstrapServers = _options.BootstrapServers;
			config.SaslUsername = _options.SaslUsername;
			config.SaslPassword = _options.SaslPassword;
			config.SaslMechanism = _options.SaslMechanism;
			config.SecurityProtocol = _options.SecurityProtocol;
		}

		public void Dispose()
		{
		}
	}
}
