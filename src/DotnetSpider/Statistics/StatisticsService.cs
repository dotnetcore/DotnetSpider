using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Extensions;
using DotnetSpider.MessageQueue;
using DotnetSpider.Statistics.Store;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IMessageQueue = DotnetSpider.MessageQueue.IMessageQueue;

namespace DotnetSpider.Statistics
{
	public class StatisticsService : BackgroundService
	{
		private readonly ILogger<StatisticsService> _logger;
		private readonly IStatisticsStore _statisticsStore;
		private readonly IMessageQueue _messageQueue;
		private AsyncMessageConsumer<byte[]> _consumer;

		public StatisticsService(ILogger<StatisticsService> logger,
			IMessageQueue messageQueue,
			IStatisticsStore statisticsStore)
		{
			_logger = logger;
			_messageQueue = messageQueue;
			_statisticsStore = statisticsStore;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Statistics service starting");
			await _statisticsStore.EnsureDatabaseAndTableCreatedAsync();

			_consumer = new AsyncMessageConsumer<byte[]>(Topics.Statistics);
			_consumer.Received += async bytes =>
			{
				var message = await bytes.DeserializeAsync(stoppingToken);
				if (message == null)
				{
					_logger.LogWarning("Received empty message");
					return;
				}

				if (message is Messages.Statistics.Success success)
				{
					await _statisticsStore.IncreaseSuccessAsync(success.SpiderId);
				}
				else if (message is Messages.Statistics.Start start)
				{
					await _statisticsStore.StartAsync(start.SpiderId, start.SpiderName);
				}
				else if (message is Messages.Statistics.Failure failure)
				{
					await _statisticsStore.IncreaseFailureAsync(failure.SpiderId);
				}
				else if (message is Messages.Statistics.Total total)
				{
					await _statisticsStore.IncreaseTotalAsync(total.SpiderId, total.Count);
				}
				else if (message is Messages.Statistics.Exit exit)
				{
					await _statisticsStore.ExitAsync(exit.SpiderId);
				}
				else if (message is Messages.Statistics.RegisterAgent registerAgent)
				{
					await _statisticsStore.RegisterAgentAsync(registerAgent.AgentId, registerAgent.AgentName);
				}
				else if (message is Messages.Statistics.AgentSuccess agentSuccess)
				{
					await _statisticsStore.IncreaseAgentSuccessAsync(agentSuccess.AgentId,
						agentSuccess.ElapsedMilliseconds);
				}
				else if (message is Messages.Statistics.AgentFailure agentFailure)
				{
					await _statisticsStore.IncreaseAgentFailureAsync(agentFailure.AgentId,
						agentFailure.ElapsedMilliseconds);
				}
				else if (message is Messages.Statistics.Print print)
				{
					var statistics = await _statisticsStore.GetSpiderStatisticsAsync(print.SpiderId);
					if (statistics != null)
					{
						var left = statistics.Total >= statistics.Success
							? (statistics.Total - statistics.Success - statistics.Failure).ToString()
							: "unknown";
						var now = DateTimeOffset.Now;
						var speed = (decimal)(statistics.Success /
						                      (now - (statistics.Start ?? now.AddMinutes(-1))).TotalSeconds);
						_logger.LogInformation(
							$"{print.SpiderId} total {statistics.Total}, speed: {decimal.Round(speed, 2)}, success {statistics.Success}, failure {statistics.Failure}, left {left}");
					}
				}
				else
				{
					var log = Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(message));
					_logger.LogWarning($"Not supported message: {log}");
				}
			};
			await _messageQueue.ConsumeAsync(_consumer, stoppingToken);
			_logger.LogInformation("Statistics service started");
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Statistics service stopping");
			_consumer?.Close();

			await base.StopAsync(cancellationToken);
			_logger.LogInformation("Statistics service stopped");
		}
	}
}
