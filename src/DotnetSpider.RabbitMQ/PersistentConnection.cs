using System;
using System.IO;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace DotnetSpider.RabbitMQ
{
	public class PersistentConnection : IDisposable
	{
		private readonly IConnectionFactory _connectionFactory;
		private readonly ILogger<PersistentConnection> _logger;
		private readonly int _retryCount;
		private	IConnection _connection;
		private	bool _disposed;
		private readonly object _syncLocker = new();

		public PersistentConnection(IConnectionFactory connectionFactory,
			ILogger<PersistentConnection> logger, int retryCount = 5)
		{
			_connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_retryCount = retryCount;
		}

		public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

		public bool TryConnect()
		{
			_logger.LogInformation("RabbitMQ Client is trying to connect");

			lock (_syncLocker)
			{
				var policy = Policy.Handle<SocketException>()
					.Or<BrokerUnreachableException>()
					.WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
						(ex, time) =>
						{
							_logger.LogWarning(ex,
								"RabbitMQ Client could not connect after {TimeOut}s ({ExceptionMessage})",
								$"{time.TotalSeconds:n1}", ex.Message);
						}
					);

				policy.Execute(() =>
				{
					_connection = _connectionFactory
						.CreateConnection();
				});

				if (IsConnected)
				{
					_connection.ConnectionShutdown += OnConnectionShutdown;
					_connection.CallbackException += OnCallbackException;
					_connection.ConnectionBlocked += OnConnectionBlocked;

					_logger.LogInformation(
						"RabbitMQ Client acquired a persistent connection to '{HostName}' and is subscribed to failure events",
						_connection.Endpoint.HostName);

					return true;
				}
				else
				{
					_logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");

					return false;
				}
			}
		}

		public IModel CreateModel()
		{
			if (!IsConnected)
			{
				throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
			}

			return _connection.CreateModel();
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			try
			{
				_connection.Dispose();
			}
			catch (IOException ex)
			{
				_logger.LogCritical(ex.ToString());
			}
		}

		private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
		{
			if (_disposed) return;

			_logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");

			TryConnect();
		}

		private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
		{
			if (_disposed) return;

			_logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");

			TryConnect();
		}

		private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
		{
			if (_disposed) return;

			_logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");

			TryConnect();
		}
	}
}
