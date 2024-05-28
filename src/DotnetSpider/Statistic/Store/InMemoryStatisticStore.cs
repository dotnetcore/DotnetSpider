using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetSpider.Statistic.Store;

public class InMemoryStatisticStore : IStatisticStore
{
    private readonly Dictionary<string, dynamic> _dict =
        new();

    private static readonly object Locker = new();

    public Task EnsureDatabaseAndTableCreatedAsync()
    {
        return Task.CompletedTask;
    }

    public Task IncreaseTotalAsync(string id, long count)
    {
        lock (Locker)
        {
            var statistics = GetOrCreate(id);
            statistics.IncrementTotal(count);
        }

        return Task.CompletedTask;
    }

    public Task IncreaseSuccessAsync(string id)
    {
        lock (Locker)
        {
            var statistics = GetOrCreate(id);
            statistics.IncrementSuccess();
        }

        return Task.CompletedTask;
    }

    public Task IncreaseFailureAsync(string id)
    {
        lock (Locker)
        {
            var statistics = GetOrCreate(id);
            statistics.IncrementFailure();
        }

        return Task.CompletedTask;
    }

    public Task StartAsync(string id, string name)
    {
        lock (Locker)
        {
            var statistics = GetOrCreate(id);
            statistics.SetName(name);
            statistics.OnStarted();
        }

        return Task.CompletedTask;
    }

    public Task ExitAsync(string id)
    {
        lock (Locker)
        {
            var statistics = GetOrCreate(id);
            statistics.OnExited();
        }

        return Task.CompletedTask;
    }

    public Task RegisterAgentAsync(string agentId, string agentName)
    {
        lock (Locker)
        {
            var statistics = GetAgentStatistics(agentId);
            statistics.SetName(agentName);
        }

        return Task.CompletedTask;
    }

    public Task IncreaseAgentSuccessAsync(string agentId, int elapsedMilliseconds)
    {
        lock (Locker)
        {
            var statistics = GetAgentStatistics(agentId);
            statistics.IncreaseSuccess();
            statistics.IncreaseElapsedMilliseconds(elapsedMilliseconds);
        }

        return Task.CompletedTask;
    }

    public Task IncreaseAgentFailureAsync(string agentId, int elapsedMilliseconds)
    {
        lock (Locker)
        {
            var statistics = GetAgentStatistics(agentId);
            statistics.IncreaseFailure();
            statistics.IncreaseElapsedMilliseconds(elapsedMilliseconds);
        }

        return Task.CompletedTask;
    }

    // public Task<PagedResult<AgentStatistic>> PagedQueryAgentStatisticAsync(string keyword, int page, int limit)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // public Task<AgentStatistic> GetAgentStatisticAsync(string id)
    // {
    //     lock (Locker)
    //     {
    //         return _dict.ContainsKey(id)
    //             ? Task.FromResult(_dict[id])
    //             : Task.FromResult<AgentStatistic>(null);
    //     }
    // }

    public Task<SpiderStatistic> GetSpiderStatisticAsync(string id)
    {
        lock (Locker)
        {
            return _dict.TryGetValue(id, out var value)
                ? (Task<SpiderStatistic>)Task.FromResult(value)
                : Task.FromResult<SpiderStatistic>(null);
        }
    }

    // public Task<PagedResult<SpiderStatistic>> PagedQuerySpiderStatisticAsync(string keyword, int page, int size)
    // {
    //     throw new NotImplementedException();
    // }

    private SpiderStatistic GetOrCreate(string id)
    {
        SpiderStatistic statistic;
        if (!_dict.TryGetValue(id, out var value))
        {
            statistic = new SpiderStatistic(id);
            _dict.Add(id, statistic);
        }
        else
        {
            statistic = value;
        }

        return statistic;
    }

    private AgentStatistic GetAgentStatistics(string id)
    {
        AgentStatistic statistic;
        if (!_dict.TryGetValue(id, out var value))
        {
            statistic = new AgentStatistic(id);
            _dict.Add(id, statistic);
        }
        else
        {
            statistic = value;
        }

        return statistic;
    }
}
