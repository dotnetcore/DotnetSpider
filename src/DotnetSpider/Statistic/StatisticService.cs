using System.Threading.Tasks;
using DotnetSpider.Extensions;
using DotnetSpider.MessageQueue;
using IMessageQueue = DotnetSpider.MessageQueue.IMessageQueue;

namespace DotnetSpider.Statistic;

public class StatisticService(IMessageQueue messageQueue) : IStatisticService
{
    public async Task IncreaseTotalAsync(string id, long count)
    {
        await messageQueue.PublishAsBytesAsync(Topics.Statistics,
            new Messages.Statistic.Total { SpiderId = id, Count = count });
    }

    public async Task IncreaseSuccessAsync(string id)
    {
        await messageQueue.PublishAsBytesAsync(Topics.Statistics,
            new Messages.Statistic.Success { SpiderId = id });
    }

    public async Task IncreaseFailureAsync(string id)
    {
        await messageQueue.PublishAsBytesAsync(Topics.Statistics,
            new Messages.Statistic.Failure { SpiderId = id });
    }

    public async Task StartAsync(string id, string name)
    {
        await messageQueue.PublishAsBytesAsync(Topics.Statistics,
            new Messages.Statistic.Start { SpiderId = id, SpiderName = name });
    }

    public async Task ExitAsync(string id)
    {
        await messageQueue.PublishAsBytesAsync(Topics.Statistics,
            new Messages.Statistic.Exit { SpiderId = id });
    }

    public async Task RegisterAgentAsync(string agentId, string agentName)
    {
        await messageQueue.PublishAsBytesAsync(Topics.Statistics,
            new Messages.Statistic.RegisterAgent { AgentId = agentId, AgentName = agentName });
    }

    public async Task IncreaseAgentSuccessAsync(string agentId, int elapsedMilliseconds)
    {
        await messageQueue.PublishAsBytesAsync(Topics.Statistics,
            new Messages.Statistic.AgentSuccess { AgentId = agentId, ElapsedMilliseconds = elapsedMilliseconds });
    }

    public async Task IncreaseAgentFailureAsync(string agentId, int elapsedMilliseconds)
    {
        await messageQueue.PublishAsBytesAsync(Topics.Statistics,
            new Messages.Statistic.AgentFailure { AgentId = agentId, ElapsedMilliseconds = elapsedMilliseconds });
    }

    public async Task PrintAsync(string id)
    {
        await messageQueue.PublishAsBytesAsync(Topics.Statistics,
            new Messages.Statistic.Print { SpiderId = id });
    }
}