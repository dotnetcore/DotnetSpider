using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace DotnetSpider.AgentCenter.Store
{
	public class InMemoryAgentStore : IAgentStore
	{
		private readonly ConcurrentDictionary<string, AgentInfo> _agentDict = new();

		public Task EnsureDatabaseAndTableCreatedAsync()
		{
			return Task.CompletedTask;
		}

		public Task<IEnumerable<AgentInfo>> GetAllListAsync()
		{
			return Task.FromResult<IEnumerable<AgentInfo>>(_agentDict.Values.ToImmutableList());
		}

		public Task RegisterAsync(AgentInfo agent)
		{
			_agentDict.TryAdd(agent.Id, agent);
			return Task.CompletedTask;
		}

		public Task HeartbeatAsync(AgentHeartbeat heartbeat)
		{
			if (_agentDict.TryGetValue(heartbeat.AgentId, out var o))
			{
				o.Refresh();
			}

			return Task.CompletedTask;
		}
	}
}
