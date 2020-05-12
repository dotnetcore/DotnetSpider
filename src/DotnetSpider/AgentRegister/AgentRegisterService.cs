using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Extensions;
using DotnetSpider.AgentRegister.Message;
using DotnetSpider.AgentRegister.Store;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SwiftMQ;

namespace DotnetSpider.AgentRegister
{
	public class AgentRegisterService : BackgroundService
	{
		private readonly ILogger<AgentRegisterService> _logger;
		private readonly IAgentStore _agentStore;
		private readonly IMessageQueue _messageQueue;
		private AsyncMessageConsumer<byte[]> _consumer;
		private readonly bool _distributed;

		public AgentRegisterService(IAgentStore agentStore, ILogger<AgentRegisterService> logger,
			IMessageQueue messageQueue)
		{
			_agentStore = agentStore;
			_logger = logger;
			_messageQueue = messageQueue;
			_distributed = !(messageQueue is MessageQueue);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Agent register service starting");

			await _agentStore.EnsureDatabaseAndTableCreatedAsync();

			_consumer = new AsyncMessageConsumer<byte[]>(TopicNames.AgentRegister);
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
						_logger.LogInformation($"Register agent: {register.Id}, {register.Name}");
					}

					await _agentStore.RegisterAsync(new AgentInfo(register.Id, register.Name, register.ProcessorCount,
						register.TotalMemory));
				}
				else if (message is Heartbeat heartbeat)
				{
					if (_distributed)
					{
						_logger.LogInformation($"Heartbeat: {heartbeat.AgentId}, {heartbeat.AgentName}");
					}

					await _agentStore.HeartbeatAsync(new AgentHeartbeat(heartbeat.AgentId, heartbeat.AgentName,
						heartbeat.FreeMemory, heartbeat.CpuLoad));
				}
				else
				{
					_logger.LogWarning($"Not supported message: {JsonConvert.SerializeObject(message)}");
				}
			};
			await _messageQueue.ConsumeAsync(_consumer, stoppingToken);
			_logger.LogInformation("Agent register service started");
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
