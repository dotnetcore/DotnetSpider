using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Extensions;
using DotnetSpider.MessageQueue;
using DotnetSpider.Statistic.Store;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IMessageQueue = DotnetSpider.MessageQueue.IMessageQueue;

namespace DotnetSpider.Statistic;

public class StatisticHostService(
    ILogger<StatisticHostService> logger,
    IMessageQueue messageQueue,
    IStatisticStore statisticStore)
    : BackgroundService
{
    private AsyncMessageConsumer<byte[]> _consumer;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogDebug("Statistic service is starting");
        await statisticStore.EnsureDatabaseAndTableCreatedAsync();

        _consumer = new AsyncMessageConsumer<byte[]>(Topics.Statistics);
        _consumer.Received += async bytes =>
        {
            var message = await bytes.DeserializeAsync(stoppingToken);
            switch (message)
            {
                case null:
                    logger.LogWarning("Received empty message");
                    return;
                case Messages.Statistic.Success success:
                    await statisticStore.IncreaseSuccessAsync(success.SpiderId);
                    break;
                case Messages.Statistic.Start start:
                    await statisticStore.StartAsync(start.SpiderId, start.SpiderName);
                    break;
                case Messages.Statistic.Failure failure:
                    await statisticStore.IncreaseFailureAsync(failure.SpiderId);
                    break;
                case Messages.Statistic.Total total:
                    await statisticStore.IncreaseTotalAsync(total.SpiderId, total.Count);
                    break;
                case Messages.Statistic.Exit exit:
                    await statisticStore.ExitAsync(exit.SpiderId);
                    break;
                case Messages.Statistic.RegisterAgent registerAgent:
                    await statisticStore.RegisterAgentAsync(registerAgent.AgentId, registerAgent.AgentName);
                    break;
                case Messages.Statistic.AgentSuccess agentSuccess:
                    await statisticStore.IncreaseAgentSuccessAsync(agentSuccess.AgentId,
                        agentSuccess.ElapsedMilliseconds);
                    break;
                case Messages.Statistic.AgentFailure agentFailure:
                    await statisticStore.IncreaseAgentFailureAsync(agentFailure.AgentId,
                        agentFailure.ElapsedMilliseconds);
                    break;
                case Messages.Statistic.Print print:
                {
                    var statistics = await statisticStore.GetSpiderStatisticAsync(print.SpiderId);
                    if (statistics != null)
                    {
                        var left = statistics.Total >= statistics.Success
                            ? (statistics.Total - statistics.Success - statistics.Failure).ToString()
                            : "-";
                        var now = DateTimeOffset.Now;
                        var speed = (decimal)(statistics.Success /
                                              (now - (statistics.Start ?? now.AddMinutes(-1))).TotalSeconds);
                        logger.LogInformation(
                            "Spider {SpiderId} total {Total}, speed: {Speed}, success {Success}, failure {Failure}, left {Left}",
                            print.SpiderId, statistics.Total, decimal.Round(speed, 2), statistics.Success,
                            statistics.Failure, left);
                    }

                    break;
                }
                default:
                {
                    var text = Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(message));
                    logger.LogWarning("Not supported message: {NotSupportedMessage}", text);
                    break;
                }
            }
        };
        await messageQueue.ConsumeAsync(_consumer);
        logger.LogDebug("Statistic service started");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Statistic service is stopping");
        _consumer?.Close();

        await base.StopAsync(cancellationToken);
        logger.LogDebug("Statistic service stopped");
    }
}
