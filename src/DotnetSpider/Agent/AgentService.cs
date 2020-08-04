using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Extensions;
using DotnetSpider.AgentRegister.Message;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftMQ;
using Exit = DotnetSpider.Agent.Message.Exit;

namespace DotnetSpider.Agent
{
	public class AgentService : BackgroundService
	{
		private readonly ILogger<AgentService> _logger;
		private readonly IMessageQueue _messageQueue;
		private readonly List<AsyncMessageConsumer<byte[]>> _consumers;
		private readonly IHostApplicationLifetime _applicationLifetime;
		private readonly bool _distributed;
		private readonly DownloaderFactory _downloaderFactory;
		private readonly AgentOptions _options;

		public AgentService(ILogger<AgentService> logger,
			IMessageQueue messageQueue,
			IOptions<AgentOptions> options,
			IHostApplicationLifetime applicationLifetime,
			DownloaderFactory downloaderFactory)
		{
			_options = options.Value;
			_logger = logger;
			_messageQueue = messageQueue;
			_applicationLifetime = applicationLifetime;
			_downloaderFactory = downloaderFactory;
			_consumers = new List<AsyncMessageConsumer<byte[]>>();
			_distributed = !(messageQueue is MessageQueue);
			if (!_distributed)
			{
				_options.AgentId = Guid.NewGuid().ToString();
				_options.AgentName = _options.AgentId;
			}
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation(_distributed ? $"Agent {_options.AgentId} starting" : "Agent starting");

			if (_distributed)
			{
				_logger.LogInformation($"Register agent: {_options.AgentId}, {_options.AgentName}");
			}

			await _messageQueue.PublishAsBytesAsync(TopicNames.AgentRegister,
				new Register
				{
					Id = _options.AgentId,
					Name = _options.AgentName,
					TotalMemory = SystemInformation.TotalMemory,
					ProcessorCount = Environment.ProcessorCount
				});

			var topics = GetDownloaderTopics();

			await RegisterAgentAsync(topics, stoppingToken);

			await Task.Factory.StartNew(async () =>
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					await HeartbeatAsync();
					await Task.Delay(5000, stoppingToken);
				}
			}, stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

			_logger.LogInformation(_distributed ? $"Agent {_options.AgentId} started" : "Agent started");
		}

		private List<string> GetDownloaderTopics()
		{
			var topics = new HashSet<string> {string.Format(TopicNames.Agent, _options.AgentId.ToUpper())};
			var downloaderNames = _downloaderFactory.GetAllDownloaderNames().ToList();
			if (!_options.SupportPuppeteer)
			{
				downloaderNames.Remove("Puppeteer");
			}

			foreach (var downloaderName in downloaderNames)
			{
				topics.Add(!string.IsNullOrWhiteSpace(_options.ADSLAccount)
					? $"{downloaderName}WithADSL"
					: downloaderName);
			}

			return topics.ToList();
		}

		private async Task RegisterAgentAsync(List<string> topics, CancellationToken stoppingToken)
		{
			foreach (var topic in topics)
			{
				var consumer = new AsyncMessageConsumer<byte[]>(topic);
				consumer.Received += HandleMessageAsync;
				await _messageQueue.ConsumeAsync(consumer, stoppingToken);
				_consumers.Add(consumer);
			}
		}

		private async Task HandleMessageAsync(byte[] bytes)
		{
			var message = await bytes.DeserializeAsync(default);
			if (message == null)
			{
				_logger.LogWarning("Received empty message");
				return;
			}

			if (message is Exit exit)
			{
				if (exit.Id == _options.AgentId)
				{
					_applicationLifetime.StopApplication();
				}
			}
			else if (message is Request request)
			{
				var downloader = _downloaderFactory.Create(request.DownloaderType);
				var response = await downloader.DownloadAsync(request);
				response.Agent = _options.AgentId;
				await _messageQueue.PublishAsBytesAsync(string.Format(TopicNames.Spider, request.Owner.ToUpper()),
					response);
				_logger.LogInformation($"{request.Owner} download {request.RequestUri}, {request.Hash} completed");
			}
			else
			{
				var log = Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(message));
				_logger.LogWarning($"Not supported message: {log}");
			}
		}

		private async Task HeartbeatAsync()
		{
			if (_distributed)
			{
				_logger.LogInformation($"Heartbeat: {_options.AgentId}, {_options.AgentName}");
			}

			await _messageQueue.PublishAsBytesAsync(TopicNames.AgentRegister,
				new Heartbeat
				{
					AgentId = _options.AgentId, AgentName = _options.AgentName, FreeMemory = 8, CpuLoad = 0
				});
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation(_distributed ? $"Agent {_options.AgentId} stopping" : "Agent stopping");
			foreach (var consumer in _consumers)
			{
				consumer.Close();
			}

			await base.StopAsync(cancellationToken);
			_logger.LogInformation(_distributed ? $"Agent {_options.AgentId} stopped" : "Agent stopped");
		}
	}
}
