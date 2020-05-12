using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Extensions;
using DotnetSpider.Statistics.Message;
using DotnetSpider.Statistics.Store;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SwiftMQ;

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

            _consumer = new AsyncMessageConsumer<byte[]>(TopicNames.Statistics);
            _consumer.Received += async bytes =>
            {
                var message = await bytes.DeserializeAsync(stoppingToken);
                if (message == null)
                {
                    _logger.LogWarning("Received empty message");
                    return;
                }

                if (message is Success success)
                {
                    await _statisticsStore.IncreaseSuccessAsync(success.Id);
                }
                else if (message is Start start)
                {
                    await _statisticsStore.StartAsync(start.Id, start.Name);
                }
                else if (message is Failure failure)
                {
                    await _statisticsStore.IncreaseFailureAsync(failure.Id);
                }
                else if (message is Total total)
                {
                    await _statisticsStore.IncreaseTotalAsync(total.Id, total.Count);
                }
                else if (message is Exit exit)
                {
                    await _statisticsStore.ExitAsync(exit.Id);
                }
                else if (message is AgentSuccess agentSuccess)
                {
                    await _statisticsStore.IncreaseAgentSuccessAsync(agentSuccess.Id, agentSuccess.ElapsedMilliseconds);
                }
                else if (message is AgentFailure agentFailure)
                {
                    await _statisticsStore.IncreaseAgentFailureAsync(agentFailure.Id, agentFailure.ElapsedMilliseconds);
                }
                else if (message is Print print)
                {
                    var statistics = await _statisticsStore.GetSpiderStatisticsAsync(print.Id);
                    if (statistics != null)
                    {
                        var left = statistics.Total >= statistics.Success
                            ? (statistics.Total - statistics.Success - statistics.Failure).ToString()
                            : "unknown";
                        _logger.LogInformation(
                            $"{print.Id} total {statistics.Total}, success {statistics.Success}, failure {statistics.Failure}, left {left}");
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
