using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetSpider.DownloadAgentRegisterCenter.Entity;

namespace DotnetSpider.DownloadAgentRegisterCenter.Store
{
	/// <summary>
	/// 本地下载器代理存储
	/// </summary>
	internal class LocalDownloaderAgentStore : IDownloaderAgentStore
	{
		private readonly ConcurrentDictionary<string, DownloaderAgent> _agents =
			new ConcurrentDictionary<string, DownloaderAgent>();

		public Task EnsureDatabaseAndTableCreatedAsync()
		{
			return Task.CompletedTask;
		}

		public Task<IEnumerable<DownloaderAgent>> GetAllListAsync()
		{
			return Task.FromResult((IEnumerable<DownloaderAgent>) _agents.Values);
		}

		public Task RegisterAsync(DownloaderAgent agent)
		{
			_agents.AddOrUpdate(agent.Id, x => agent, (s, a) =>
			{
				a.CreationTime = DateTime.Now;
				a.LastModificationTime = DateTime.Now;
				return a;
			});
			return Task.CompletedTask;
		}

		/// <summary>
		/// 本地代理不需要留存心跳
		/// </summary>
		/// <param name="heartbeat"></param>
		/// <returns></returns>
		public Task HeartbeatAsync(DownloaderAgentHeartbeat heartbeat)
		{
			if (_agents.TryGetValue(heartbeat.AgentId, out var agent))
			{
				agent.LastModificationTime = DateTime.Now;
			}

			return Task.CompletedTask;
		}
	}
}
