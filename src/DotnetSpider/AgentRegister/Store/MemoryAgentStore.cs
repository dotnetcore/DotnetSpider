using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DotnetSpider.AgentRegister.Store
{
    public class MemoryAgentStore : IAgentStore
    {
        private readonly Dictionary<string, AgentInfo> _agentDict = new Dictionary<string, AgentInfo>();

        public Task EnsureDatabaseAndTableCreatedAsync()
        {
            return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Task<IEnumerable<AgentInfo>> GetAllListAsync()
        {
            return Task.FromResult<IEnumerable<AgentInfo>>(_agentDict.Values.ToImmutableList());
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Task RegisterAsync(AgentInfo agent)
        {
            if (!_agentDict.ContainsKey(agent.Id))
            {
                _agentDict.Add(agent.Id, agent);
            }

            return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Task HeartbeatAsync(AgentHeartbeat heartbeat)
        {
            if (_agentDict.ContainsKey(heartbeat.AgentId))
            {
                _agentDict[heartbeat.AgentId].Refresh();
            }

            return Task.CompletedTask;
        }
    }
}