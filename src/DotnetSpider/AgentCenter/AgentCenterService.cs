using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.AgentCenter.Store;
using DotnetSpider.Extensions;
using DotnetSpider.Message.Agent;
using DotnetSpider.MessageQueue;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IMessageQueue = DotnetSpider.MessageQueue.IMessageQueue;

namespace DotnetSpider.AgentCenter
{
	public class AgentCenterService : BackgroundService
	{
		private readonly ILogger<AgentCenterService> _logger;
		private readonly IAgentStore _agentStore;
		private readonly IMessageQueue _messageQueue;
		private AsyncMessageConsumer<byte[]> _consumer;
		private readonly bool _distributed;

		public AgentCenterService(IAgentStore agentStore, ILogger<AgentCenterService> logger,
			IMessageQueue messageQueue)
		{
			_agentStore = agentStore;
			_logger = logger;
			_messageQueue = messageQueue;
			_distributed = !(messageQueue is MessageQueue.MessageQueue);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				_logger.LogInformation("Agent center service is starting");

				await _agentStore.EnsureDatabaseAndTableCreatedAsync();

				_consumer = new AsyncMessageConsumer<byte[]>(TopicNames.AgentCenter);
				_consumer.Received += async bytes =>
				{
					var message = await bytes.DeserializeAsync(stoppingToken);
					if (message == null)
					{
						_logger.LogWarning("Received empty message");
						return;
					}

					if (message is Register register)
					{
						if (_distributed)
						{
							_logger.LogInformation($"Register agent: {register.AgentId}, {register.AgentName}");
						}

						await _agentStore.RegisterAsync(new AgentInfo(register.AgentId, register.AgentName,
							register.ProcessorCount,
							register.TotalMemory));
					}
					else if (message is Heartbeat heartbeat)
					{
						if (_distributed)
						{
							_logger.LogInformation($"Receive heartbeat: {heartbeat.AgentId}, {heartbeat.AgentName}");
						}

						await _agentStore.HeartbeatAsync(new AgentHeartbeat(heartbeat.AgentId, heartbeat.AgentName,
							heartbeat.FreeMemory, heartbeat.CpuLoad));
					}
					else
					{
						var msg = Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(message));
						_logger.LogWarning($"Not supported message: {msg}");
					}
				};
				await _messageQueue.ConsumeAsync(_consumer, stoppingToken);
				_logger.LogInformation("Agent center service started");
			}
			catch (Exception e)
			{
				_logger.LogCritical(e.ToString());
			}
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Agent register service stopping");
			_consumer?.Close();

			await base.StopAsync(cancellationToken);
			_logger.LogInformation("Agent register service stopped");
		}
	}
}
