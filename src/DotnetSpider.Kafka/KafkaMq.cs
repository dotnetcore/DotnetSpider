using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using DotnetSpider.Common;
using DotnetSpider.MessageQueue;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Logging;
using Partitioner = Confluent.Kafka.Partitioner;

namespace DotnetSpider.Kafka
{
	/// <summary>
	/// TODO: 确定 Kafka 这个客户端在网络断开、Kafka 崩溃情况下的表现，是否一直等待还是抛异常退出
	/// TODO: 如果会退出，则需要重试
	/// </summary>
	public class KafkaMq : IMq
	{
		private readonly ConcurrentDictionary<string, IConsumer<Null, byte[]>> _consumers =
			new ConcurrentDictionary<string, IConsumer<Null, byte[]>>();

		private readonly ILogger _logger;
		private readonly IProducer<Null, byte[]> _producer;
		private readonly KafkaOptions _options;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="options">爬虫选项</param>
		/// <param name="logger">日志接口</param>
		public KafkaMq(KafkaOptions options,
			ILogger<KafkaMq> logger)
		{
			_logger = logger;
			_options = options;
			var productConfig = new ProducerConfig
			{
				Partitioner = Partitioner.ConsistentRandom, CompressionType = CompressionType.Lz4
			};
			SetClientConfig(productConfig);
			var builder =
				new ProducerBuilder<Null, byte[]>(productConfig);

			_producer = builder.Build();
		}

		public async Task PublishAsync<TData>(string topic, MessageData<TData> message)
		{
			if (message == null)
			{
#if DEBUG
				var stackTrace = new System.Diagnostics.StackTrace();
				_logger.LogDebug($"Publish empty message to topic {topic}: {stackTrace}");
#endif
				return;
			}


			await _producer.ProduceAsync(topic,
				new Message<Null, byte[]>
				{
					Value = LZ4MessagePackSerializer.Serialize(
						new TransferMessage
						{
							Timestamp = (long)DateTimeHelper.GetCurrentUnixTimeNumber(),
							Type = message.Type,
							Data = LZ4MessagePackSerializer.Serialize(message.Data,
								TypelessContractlessStandardResolver.Instance)
						}, TypelessContractlessStandardResolver.Instance)
				});
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Subscribe<TData>(string topic, Action<MessageData<TData>> action)
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
			var consumer = new ConsumerBuilder<Null, byte[]>(config).Build();
			consumer.Subscribe(topic);
			_consumers.TryAdd(topic, consumer);
			Task.Factory.StartNew(() =>
			{
				_logger.LogInformation("Subscribe: " + topic);
				while (_consumers.ContainsKey(topic))
				{
					TransferMessage msg = null;
					try
					{
						var value = consumer.Consume().Value;
						msg = LZ4MessagePackSerializer.Deserialize<TransferMessage>(value,
							TypelessContractlessStandardResolver.Instance);
					}
					catch (ObjectDisposedException)
					{
						_logger?.LogDebug("Kafka handler is disposed");
					}
					catch (Exception e)
					{
						_logger?.LogError($"Consume kafka message failed on topic {topic}: {e}");
					}

					if (msg != null)
					{
						Task.Factory.StartNew(() =>
						{
							action?.Invoke(new MessageData<TData>
							{
								Timestamp = msg.Timestamp,
								Type = msg.Type,
								Data = LZ4MessagePackSerializer.Deserialize<TData>(msg.Data,
									TypelessContractlessStandardResolver.Instance)
							});
						}).ContinueWith(t =>
						{
							if (t.Exception != null)
							{
								_logger?.LogError($"Handle kafka message failed on topic {topic}: {t}");
							}
						});
					}
					else
					{
						_logger?.LogWarning($"Ignore empty kafka message on topic {topic}");
					}
				}

				_logger?.LogWarning($"Exit consume kafka topic {topic}");
			}).ConfigureAwait(true);
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
					{
						NumPartitions = _options.TopicPartitionCount, Name = topic, ReplicationFactor = 1
					}
				}).GetAwaiter().GetResult();
			}
			else
			{
				if (partitionCount < _options.TopicPartitionCount)
				{
					adminClient.CreatePartitionsAsync(new[]
					{
						new PartitionsSpecification {Topic = topic, IncreaseTo = _options.TopicPartitionCount}
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
