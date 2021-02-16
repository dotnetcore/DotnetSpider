using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetSpider.Infrastructure;

namespace DotnetSpider.Statistics.Store
{
	public class DefaultStatisticsStore : IStatisticsStore
	{
		private readonly Dictionary<string, dynamic> _dict =
			new();

		private static readonly object _locker = new();

		public Task EnsureDatabaseAndTableCreatedAsync()
		{
			return Task.CompletedTask;
		}

		public Task IncreaseTotalAsync(string id, long count)
		{
			lock (_locker)
			{
				var statistics = GetSpiderStatistics(id);
				statistics.IncrementTotal(count);
			}

			return Task.CompletedTask;
		}

		public Task IncreaseSuccessAsync(string id)
		{
			lock (_locker)
			{
				var statistics = GetSpiderStatistics(id);
				statistics.IncrementSuccess();
			}

			return Task.CompletedTask;
		}

		public Task IncreaseFailureAsync(string id)
		{
			lock (_locker)
			{
				var statistics = GetSpiderStatistics(id);
				statistics.IncrementFailure();
			}

			return Task.CompletedTask;
		}

		public Task StartAsync(string id, string name)
		{
			lock (_locker)
			{
				var statistics = GetSpiderStatistics(id);
				statistics.SetName(name);
				statistics.OnStarted();
			}

			return Task.CompletedTask;
		}

		public Task ExitAsync(string id)
		{
			lock (_locker)
			{
				var statistics = GetSpiderStatistics(id);
				statistics.OnExited();
			}

			return Task.CompletedTask;
		}

		public Task RegisterAgentAsync(string agentId, string agentName)
		{
			lock (_locker)
			{
				var statistics = GetAgentStatistics(agentId);
				statistics.SetName(agentName);
			}

			return Task.CompletedTask;
		}

		public Task IncreaseAgentSuccessAsync(string agentId, int elapsedMilliseconds)
		{
			lock (_locker)
			{
				var statistics = GetAgentStatistics(agentId);
				statistics.IncreaseSuccess();
				statistics.IncreaseElapsedMilliseconds(elapsedMilliseconds);
			}

			return Task.CompletedTask;
		}

		public Task IncreaseAgentFailureAsync(string agentId, int elapsedMilliseconds)
		{
			lock (_locker)
			{
				var statistics = GetAgentStatistics(agentId);
				statistics.IncreaseFailure();
				statistics.IncreaseElapsedMilliseconds(elapsedMilliseconds);
			}

			return Task.CompletedTask;
		}

		public Task<PagedResult<AgentStatistics>> PagedQueryAgentStatisticsAsync(string keyword, int page, int limit)
		{
			throw new NotImplementedException();
		}

		public Task<AgentStatistics> GetAgentStatisticsAsync(string id)
		{
			lock (_locker)
			{
				return _dict.ContainsKey(id)
					? Task.FromResult(_dict[id])
					: Task.FromResult<AgentStatistics>(null);
			}
		}

		public Task<SpiderStatistics> GetSpiderStatisticsAsync(string id)
		{
			lock (_locker)
			{
				return _dict.ContainsKey(id)
					? (Task<SpiderStatistics>)Task.FromResult(_dict[id])
					: Task.FromResult<SpiderStatistics>(null);
			}
		}

		public Task<PagedResult<SpiderStatistics>> PagedQuerySpiderStatisticsAsync(string keyword, int page, int size)
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
