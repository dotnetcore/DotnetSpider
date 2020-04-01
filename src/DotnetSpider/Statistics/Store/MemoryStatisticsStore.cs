using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DotnetSpider.Infrastructure;

namespace DotnetSpider.Statistics.Store
{
    public class MemoryStatisticsStore : IStatisticsStore
    {
        private readonly Dictionary<string, dynamic> _dict =
            new Dictionary<string, dynamic>();

        public Task EnsureDatabaseAndTableCreatedAsync()
        {
            return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Task IncreaseTotalAsync(string id, long count)
        {
            var statistics = GetSpiderStatistics(id);
            statistics.IncrementTotal(count);
            return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Task IncreaseSuccessAsync(string id)
        {
            var statistics = GetSpiderStatistics(id);
            statistics.IncrementSuccess();
            return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Task IncreaseFailureAsync(string id)
        {
            var statistics = GetSpiderStatistics(id);
            statistics.IncrementFailure();
            return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Task StartAsync(string id, string name)
        {
            var statistics = GetSpiderStatistics(id);
            statistics.SetName(name);
            statistics.OnStarted();
            return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Task ExitAsync(string id)
        {
            var statistics = GetSpiderStatistics(id);
            statistics.OnExited();
            return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Task RegisterAgentAsync(string agentId, string agentName)
        {
	        var statistics = GetAgentStatistics(agentId);
	        statistics.SetName(agentName);
	        return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Task IncreaseAgentSuccessAsync(string agentId, int elapsedMilliseconds)
        {
            var statistics = GetAgentStatistics(agentId);
            statistics.IncreaseSuccess();
            statistics.IncreaseElapsedMilliseconds(elapsedMilliseconds);
            return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Task IncreaseAgentFailureAsync(string agentId, int elapsedMilliseconds)
        {
            var statistics = GetAgentStatistics(agentId);
            statistics.IncreaseFailure();
            statistics.IncreaseElapsedMilliseconds(elapsedMilliseconds);
            return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Task<PagedQueryResult<AgentStatistics>> PagedQueryAgentStatisticsAsync(string keyword, int page, int limit)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Task<AgentStatistics> GetAgentStatisticsAsync(string id)
        {
	        return _dict.ContainsKey(id)
                ?   Task.FromResult(_dict[id])
                : Task.FromResult<AgentStatistics>(null);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Task<SpiderStatistics> GetSpiderStatisticsAsync(string id)
        {
            return _dict.ContainsKey(id)
                ? (Task<SpiderStatistics>) Task.FromResult(_dict[id])
                : Task.FromResult<SpiderStatistics>(null);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Task<PagedQueryResult<SpiderStatistics>> PagedQuerySpiderStatisticsAsync(string keyword, int page, int size)
        {
            throw new NotImplementedException();
        }

        private SpiderStatistics GetSpiderStatistics(string id)
        {
            SpiderStatistics statistics;
            if (!_dict.ContainsKey(id))
            {
                statistics = new SpiderStatistics(id);
                _dict.Add(id, statistics);
            }
            else
            {
                statistics = _dict[id];
            }

            return statistics;
        }

        private AgentStatistics GetAgentStatistics(string id)
        {
            AgentStatistics statistics;
            if (!_dict.ContainsKey(id))
            {
                statistics = new AgentStatistics(id);
                _dict.Add(id, statistics);
            }
            else
            {
                statistics = _dict[id];
            }

            return statistics;
        }
    }
}
