using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.Downloader.Entity;

namespace DotnetSpider.Downloader.Internal
{
	/// <summary>
	/// 本地下载器代理存储
	/// </summary>
    internal class LocalDownloaderAgentStore : IDownloaderAgentStore
    {
        private readonly ConcurrentDictionary<string, DownloaderAgent> _agents =
            new ConcurrentDictionary<string, DownloaderAgent>();

        private readonly ConcurrentDictionary<string, IEnumerable<string>> _allocatedAgents =
            new ConcurrentDictionary<string, IEnumerable<string>>();

        public Task<List<DownloaderAgent>> GetAllListAsync()
        {
            return Task.FromResult(_agents.Values.ToList());
        }

        public Task<List<DownloaderAgent>> GetAllListAsync(string ownerId)
        {
            if (_allocatedAgents.TryGetValue(ownerId, out var ids))
            {
                var agents = _agents.Where(x => ids.Contains(x.Key)).Select(x => x.Value).ToList();
                return Task.FromResult(agents);
            }

            return null;
        }

        public Task RegisterAsync(DownloaderAgent agent)
        {
            _agents.AddOrUpdate(agent.Id, x => agent, (s, a) =>
            {
                a.LastModificationTime = DateTime.Now;
                return a;
            });
#if NETFRAMEWORK
            return DotnetSpider.Core.Framework.CompletedTask;
#else
            return Task.CompletedTask;
#endif
        }

        /// <summary>
        /// 本地代理不需要留存心跳
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public Task HeartbeatAsync(DownloaderAgentHeartbeat agent)
        {
#if NETFRAMEWORK
            return DotnetSpider.Core.Framework.CompletedTask;
#else
            return Task.CompletedTask;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="agentIds"></param>
        /// <returns></returns>
        public Task AllocateAsync(string ownerId, IEnumerable<string> agentIds)
        {
            var agentIdArray = agentIds.ToArray();
            _allocatedAgents.AddOrUpdate(ownerId, s => agentIdArray, (s, v) => agentIdArray);

#if NETFRAMEWORK
            return DotnetSpider.Core.Framework.CompletedTask;
#else
            return Task.CompletedTask;
#endif
        }
    }
}