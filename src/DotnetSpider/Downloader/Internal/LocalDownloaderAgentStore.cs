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

		private readonly ConcurrentDictionary<string, string> _allocateDownloaderMessages =
			new ConcurrentDictionary<string, string>();

		public Task EnsureDatabaseAndTableCreatedAsync()
		{
#if NETFRAMEWORK
            return DotnetSpider.Core.Framework.CompletedTask;
#else
			return Task.CompletedTask;
#endif
		}

		public Task<List<DownloaderAgent>> GetAllListAsync()
		{
			return Task.FromResult(_agents.Values.ToList());
		}

		public Task<List<DownloaderAgentAllocate>> GetAllocatedListAsync(string ownerId)
		{
			if (_allocatedAgents.TryGetValue(ownerId, out var ids))
			{
				var agents = _agents.Where(x => ids.Contains(x.Key)).Select(x => new DownloaderAgentAllocate
				{
					AgentId = x.Value.Id,
					OwnerId = ownerId
				}).ToList();
				return Task.FromResult(agents);
			}

			return null;
		}

		public Task RegisterAsync(DownloaderAgent agent)
		{
			_agents.AddOrUpdate(agent.Id, x => agent, (s, a) =>
			{
				a.CreationTime = DateTime.Now;
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
		/// 保存给任务分配的下载器代理
		/// </summary>
		/// <param name="ownerId">任务标识</param>
		/// <param name="message">分配下载器代理的消息</param>
		/// <param name="agentIds">分配的下载器代理标识</param>
		/// <returns></returns>
		public Task AllocateAsync(string ownerId, string message, IEnumerable<string> agentIds)
		{
			var agentIdArray = agentIds.ToArray();
			_allocatedAgents.AddOrUpdate(ownerId, s => agentIdArray, (s, v) => agentIdArray);
			_allocateDownloaderMessages.AddOrUpdate(ownerId, x => message, (s, x) => x);
#if NETFRAMEWORK
            return DotnetSpider.Core.Framework.CompletedTask;
#else
			return Task.CompletedTask;
#endif
		}

		public Task<string> GetAllocateDownloaderMessageAsync(string ownerId)
		{
			if (_allocateDownloaderMessages.TryGetValue(ownerId, out string message))
			{
				return Task.FromResult(message);
			}

			return Task.FromResult(string.Empty);
		}
	}
}