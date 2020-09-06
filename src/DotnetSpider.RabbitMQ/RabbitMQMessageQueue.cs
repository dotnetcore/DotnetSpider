using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.MessageQueue;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;


[assembly: InternalsVisibleTo("DotnetSpider.Tests")]

namespace DotnetSpider.RabbitMQ
{
	public class RabbitMQMessageQueue : IMessageQueue
	{
		private readonly RabbitMQOptions _options;
		private readonly PersistentConnection _connection;
		private readonly ILogger<RabbitMQMessageQueue> _logger;

		public RabbitMQMessageQueue(IOptions<RabbitMQOptions> options, ILoggerFactory loggerFactory)
		{
			_logger = loggerFactory.CreateLogger<RabbitMQMessageQueue>();
			_options = options.Value;
			_connection = new PersistentConnection(CreateConnectionFactory(),
				loggerFactory.CreateLogger<PersistentConnection>(), _options.RetryCount);
			CreateConsumerChannel();
		}

		private IConnectionFactory CreateConnectionFactory()
		{
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

			return connectionFactory;
		}

		public async Task PublishAsync(string topic, byte[] bytes)
		{
			if (bytes == null)
			{
				throw new ArgumentNullException(nameof(bytes));
			}

			if (!_connection.IsConnected)
			{
				_connection.TryConnect();
			}

			var policy = Policy.Handle<BrokerUnreachableException>()
				.Or<SocketException>()
				.WaitAndRetry(_options.RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
					(ex, time) =>
					{
						_logger.LogWarning(ex,
							"Could not publish data after {Timeout}s ({ExceptionMessage})",
							$"{time.TotalSeconds:n1}", ex.Message);
					});
			var channel = _connection.CreateModel();

			_logger.LogTrace("Declaring RabbitMQ exchange to publish event");

			channel.ExchangeDeclare(exchange: _options.Exchange, type: "direct");

			policy.Execute(() =>
			{
				var properties = channel.CreateBasicProperties();
				properties.DeliveryMode = 2; // persistent

				_logger.LogTrace("Publishing event to RabbitMQ");

				channel.BasicPublish(_options.Exchange, topic, true, properties, bytes);
				channel.Dispose();
			});

			await Task.CompletedTask;
		}

		public void CloseQueue(string queue)
		{
			using var channel = _connection.CreateModel();
			channel.QueueDelete(queue);
		}

		public Task ConsumeAsync(AsyncMessageConsumer<byte[]> consumer,
			CancellationToken stoppingToken)
		{
			if (consumer.Registered)
			{
				throw new ApplicationException("This consumer is already registered");
			}

			if (!_connection.IsConnected)
			{
				_connection.TryConnect();
			}

			var channel = _connection.CreateModel();
			var eventingBasicConsumer = new AsyncEventingBasicConsumer(channel);
			var queue = channel.QueueDeclare().QueueName;
			var topic = consumer.Queue;
			channel.QueueBind(queue: queue, _options.Exchange, routingKey: topic);
			eventingBasicConsumer.Received += async (model, ea) =>
			{
				await consumer.InvokeAsync(ea.Body.ToArray());
				channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
			};
			consumer.OnClosing += x =>
			{
				channel.Close();
			};
			//7. 启动消费者
			channel.BasicConsume(queue: queue, autoAck: false, consumer: eventingBasicConsumer);

			return Task.CompletedTask;
		}

		private void CreateConsumerChannel()
		{
			if (!_connection.IsConnected)
			{
				_connection.TryConnect();
			}

			_logger.LogTrace("Creating RabbitMQ consumer channel");

			var channel = _connection.CreateModel();
			channel.ExchangeDeclare(exchange: _options.Exchange, "direct");
		}
	}
}
