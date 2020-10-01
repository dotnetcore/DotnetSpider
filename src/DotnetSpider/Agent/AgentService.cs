using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Downloader;
using DotnetSpider.Extensions;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using DotnetSpider.Message.Agent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Exit = DotnetSpider.Message.Agent.Exit;
using IMessageQueue = DotnetSpider.MessageQueue.IMessageQueue;

namespace DotnetSpider.Agent
{
	public class AgentService : BackgroundService
	{
		private readonly ILogger<AgentService> _logger;
		private readonly IMessageQueue _messageQueue;
		private readonly List<MessageQueue.AsyncMessageConsumer<byte[]>> _consumers;
		private readonly IHostApplicationLifetime _applicationLifetime;
		private readonly IDownloader _downloader;
		private readonly AgentOptions _options;

		public AgentService(ILogger<AgentService> logger,
			IMessageQueue messageQueue,
			IOptions<AgentOptions> options,
			IHostApplicationLifetime applicationLifetime,
			IDownloader downloader)
		{
			_options = options.Value;
			_logger = logger;
			_messageQueue = messageQueue;
			_applicationLifetime = applicationLifetime;
			_downloader = downloader;
			_consumers = new List<MessageQueue.AsyncMessageConsumer<byte[]>>();
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation(
				_messageQueue.IsDistributed
					? $"Agent {_options.AgentId}, {_options.AgentName} is starting"
					: "Agent is starting");

			// 分布式才需要注册
			if (_messageQueue.IsDistributed)
			{
				await _messageQueue.PublishAsBytesAsync(TopicNames.AgentCenter,
					new Register
					{
						AgentId = _options.AgentId,
						AgentName = _options.AgentName,
						TotalMemory = SystemInformation.TotalMemory,
						ProcessorCount = Environment.ProcessorCount
					});
			}

			// 节点注册对应的 topic 才会收到下载的请求
			// agent_{id} 这是用于指定节点下载
			// httpclient 这是指定下载器
			await RegisterAgentAsync(_downloader.Name, stoppingToken);

			if (_messageQueue.IsDistributed)
			{
				await RegisterAgentAsync(string.Format(TopicNames.Spider, _options.AgentId), stoppingToken);
			}

			// 分布式才需要发送心跳
			if (_messageQueue.IsDistributed)
			{
				await Task.Factory.StartNew(async () =>
				{
					while (!stoppingToken.IsCancellationRequested)
					{
						await HeartbeatAsync();
						await Task.Delay(5000, stoppingToken);
					}
				}, stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
			}

			_logger.LogInformation(_messageQueue.IsDistributed
				? $"Agent {_options.AgentId}, {_options.AgentName} started"
				: "Agent started");
		}

		private async Task RegisterAgentAsync(string topic, CancellationToken stoppingToken)
		{
			var consumer = new MessageQueue.AsyncMessageConsumer<byte[]>(topic);
			consumer.Received += HandleMessageAsync;
			await _messageQueue.ConsumeAsync(consumer, stoppingToken);
			_consumers.Add(consumer);
		}

		private async Task HandleMessageAsync(byte[] bytes)
		{
			var message = await GetMessageAsync(bytes);
			if (message == null)
			{
				return;
			}

			if (message is Exit exit)
			{
				if (exit.AgentId == _options.AgentId)
				{
					_applicationLifetime.StopApplication();
				}
			}
			else if (message is Request request)
			{
				Task.Factory.StartNew(async () =>
				{
					var response = await _downloader.DownloadAsync(request);
					if (response == null)
					{
						return;
					}

					response.Agent = _options.AgentId;

					var topic = string.Format(TopicNames.Spider, request.Owner.ToUpper());
					await _messageQueue.PublishAsBytesAsync(topic, response);

					_logger.LogInformation(
						_messageQueue.IsDistributed
							? $"Agent {_options.AgentName} download {request.RequestUri}, {request.Hash} for {request.Owner} completed"
							: $"{request.Owner} download {request.RequestUri}, {request.Hash} completed");
				}).ConfigureAwait(false).GetAwaiter();
			}
			else
			{
				var msg = Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(message));
				_logger.LogWarning($"Not supported message: {msg}");
			}
		}

		private async Task HeartbeatAsync()
		{
			_logger.LogInformation($"Heartbeat {_options.AgentId}, {_options.AgentName}");

			await _messageQueue.PublishAsBytesAsync(TopicNames.AgentCenter,
				new Heartbeat
				{
					AgentId = _options.AgentId,
					AgentName = _options.AgentName,
					FreeMemory = SystemInformation.FreeMemory,
					CpuLoad = 0
				});
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				_messageQueue.IsDistributed ? $"Agent {_options.AgentId} is stopping" : "Agent is stopping");

			foreach (var consumer in _consumers)
			{
				consumer.Close();
			}

			await base.StopAsync(cancellationToken);

			_logger.LogInformation(_messageQueue.IsDistributed ? $"Agent {_options.AgentId} stopped" : "Agent stopped");
		}

		private async Task<object> GetMessageAsync(byte[] bytes)
		{
			try
			{
				return await bytes.DeserializeAsync();
			}
			catch (Exception e)
			{
				_logger.LogError($"Deserialize message failed: {e}");
				return null;
			}
		}
	}
}
