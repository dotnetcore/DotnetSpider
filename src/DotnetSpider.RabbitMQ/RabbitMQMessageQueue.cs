using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Extensions;
using MessagePack;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using SwiftMQ;

namespace DotnetSpider.RabbitMQ
{
	public class RabbitMQMessageQueue : IMessageQueue
	{
		private readonly RabbitMQOptions _options;
		private readonly IConnection _connection;
		private readonly ConcurrentDictionary<string, IModel> _modelDict;

		public RabbitMQMessageQueue(IOptions<RabbitMQOptions> options) : this(options.Value)
		{
		}

		public RabbitMQMessageQueue(RabbitMQOptions options)
		{
			_options = options;
			var connectionFactory = new ConnectionFactory {HostName = _options.HostName, DispatchConsumersAsync = true};
			if (_options.Port > 0)
			{
				connectionFactory.Port = _options.Port;
			}

			if (!string.IsNullOrWhiteSpace(_options.UserName))
			{
				connectionFactory.UserName = _options.UserName;
			}

			if (!string.IsNullOrWhiteSpace(_options.Password))
			{
				connectionFactory.Password = _options.Password;
			}

			_connection = connectionFactory.CreateConnection();
			_modelDict = new ConcurrentDictionary<string, IModel>();
		}

		public async Task PublishAsync<TMessage>(string queue, TMessage message)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			var channel = _modelDict.GetOrAdd(queue, CreateChannel);
			var bytes = message as byte[] ?? MessagePackSerializer.Typeless.Serialize(message);
			if (channel.IsOpen)
			{
				channel.BasicPublish(_options.Exchange, queue, null, bytes);
			}

			await Task.CompletedTask;
		}

		public Task ConsumeAsync<TMessage>(AsyncMessageConsumer<TMessage> consumer,
			CancellationToken stoppingToken)
		{
			var topic = consumer.Queue;
			var channel = _modelDict.GetOrAdd(topic, CreateChannel);
			var consumer1 = new AsyncEventingBasicConsumer(channel);
			var queue = channel.QueueDeclare().QueueName;
			channel.QueueBind(queue: queue, _options.Exchange, routingKey: topic);
			consumer1.Received += async (model, ea) =>
			{
				if (consumer is AsyncMessageConsumer<byte[]> bytesConsumer)
				{
					await bytesConsumer.InvokeAsync(ea.Body);
				}
				else
				{
					var message = (TMessage)await ea.Body.DeserializeAsync(stoppingToken);
					if (message != null)
					{
						await consumer.InvokeAsync(message);
					}
				}

				channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
			};

			consumer.OnClosing += () =>
			{
				channel.Close();
			};
			//7. 启动消费者
			channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer1);

			return Task.CompletedTask;
		}

		private IModel CreateChannel(string topic)
		{
			var channel = _connection.CreateModel();
			channel.QueueDeclare(topic, durable: true, exclusive: false, autoDelete: false,
				arguments: null);
			channel.ExchangeDeclare(exchange: _options.Exchange, type: "direct", durable: true);
			return channel;
		}
	}
}
