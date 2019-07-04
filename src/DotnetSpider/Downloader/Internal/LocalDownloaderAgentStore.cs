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
		private readonly ConcurrentDictionary<string, Entity.DownloaderAgent> _agents =
			new ConcurrentDictionary<string, Entity.DownloaderAgent>();

		public Task EnsureDatabaseAndTableCreatedAsync()
		{
			return Task.CompletedTask;
		}

		public Task<IEnumerable<Entity.DownloaderAgent>> GetAllListAsync()
		{
			return Task.FromResult((IEnumerable<Entity.DownloaderAgent>) _agents.Values);
		}

		public Task RegisterAsync(Entity.DownloaderAgent agent)
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