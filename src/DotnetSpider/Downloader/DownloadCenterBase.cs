using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Downloader.Entity;
using DotnetSpider.MessageQueue;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 下载中心
	/// </summary>
	public abstract class DownloadCenterBase : IDownloadCenter
	{
		private bool _isRunning;

		private readonly ConcurrentDictionary<string, DownloaderAgent> _agents =
			new ConcurrentDictionary<string, DownloaderAgent>();

		private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, object>> _allocatedAgents =
			new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>();

		/// <summary>
		/// 消息队列
		/// </summary>
		protected readonly IMessageQueue Mq;

		/// <summary>
		/// 日志接口
		/// </summary>
		protected readonly ILogger Logger;

		/// <summary>
		/// 下载器代理存储
		/// </summary>
		protected readonly IDownloaderAgentStore DownloaderAgentStore;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="mq">消息队列</param>
		/// <param name="downloaderAgentStore">下载器代理存储</param>
		/// <param name="logger">日志接口</param>
		protected DownloadCenterBase(
			IMessageQueue mq,
			IDownloaderAgentStore downloaderAgentStore,
			ILogger logger)
		{
			Mq = mq;
			DownloaderAgentStore = downloaderAgentStore;
			Logger = logger;
		}

		/// <summary>
		/// 启动下载中心
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <exception cref="SpiderException"></exception>
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			if (_isRunning)
			{
				throw new SpiderException("下载中心正在运行中");
			}

			await DownloaderAgentStore.EnsureDatabaseAndTableCreatedAsync();

			await SyncDownloaderAgentData();

			StartSyncDownloaderAgentDataService().ConfigureAwait(false).GetAwaiter();

			Mq.Subscribe(Framework.DownloaderCenterTopic, async message =>
			{
				var commandMessage = message.ToCommandMessage();
				if (commandMessage == null)
				{
					Logger.LogWarning($"接收到非法消息: {message}");
					return;
				}

				switch (commandMessage.Command)
				{
					case Framework.RegisterCommand:
					{
						var agent = JsonConvert.DeserializeObject<DownloaderAgent>(commandMessage.Message);
						if ((DateTime.Now - agent.CreationTime).TotalSeconds < 30)
						{
							_agents.AddOrUpdate(agent.Id, x => agent, (s, a) =>
							{
								a.LastModificationTime = agent.LastModificationTime;
								return a;
							});
							await DownloaderAgentStore.RegisterAsync(agent);
							Logger.LogInformation($"注册下载代理器 {agent.Id} 成功");
						}
						else
						{
							Logger.LogWarning($"注册下载代理器 {agent.Id} 过期");
						}

						break;
					}
					case Framework.HeartbeatCommand:
					{
						var heartbeat = JsonConvert.DeserializeObject<DownloaderAgentHeartbeat>(commandMessage.Message);
						if ((DateTime.Now - heartbeat.CreationTime).TotalSeconds < 30)
						{
							if (_agents.ContainsKey(heartbeat.AgentId))
							{
								_agents[heartbeat.AgentId].RefreshLastModificationTime();
							}

							await DownloaderAgentStore.HeartbeatAsync(heartbeat);
							Logger.LogDebug($"注册下载代理器 {heartbeat.AgentId} 更新心跳成功");
						}
						else
						{
							Logger.LogWarning($"注册下载代理器 {heartbeat.AgentId} 更新心跳过期");
						}

						break;
					}
					case Framework.AllocateDownloaderCommand:
					{
						var options = JsonConvert.DeserializeObject<AllotDownloaderMessage>(commandMessage.Message);
						if ((DateTime.Now - options.CreationTime).TotalSeconds < 30)
						{
							await AllocateAsync(options);
							await Mq.PublishAsync($"{Framework.ResponseHandlerTopic}{options.OwnerId}",
								$"|{Framework.AllocateDownloaderCommand}|true");
						}
						else
						{
							Logger.LogWarning($"任务 {options.OwnerId} 分配下载代理器过期");
						}

						break;
					}
					case Framework.DownloadCommand:
					{
						var requests = JsonConvert.DeserializeObject<Request[]>(commandMessage.Message);
						requests = requests.Where(x => (DateTime.Now - x.CreationTime).TotalSeconds < 60).ToArray();
						if (requests.Length > 0)
						{
							var ownerId = requests[0].OwnerId;
							foreach (var request in requests)
							{
								request.CreationTime = DateTime.Now;
							}

							await EnqueueRequests(ownerId, requests);
							Logger.LogDebug($"任务 {ownerId} 下载请求分发成功");
						}

						break;
					}
				}
			});
			Logger.LogInformation("下载中心启动完毕");
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			Mq.Unsubscribe(Framework.DownloaderCenterTopic);
			_isRunning = false;
			Logger.LogInformation("下载中心退出");
#if NETFRAMEWORK
			return Framework.CompletedTask;
#else
			return Task.CompletedTask;
#endif
		}

		
		/// <summary>
		/// 分配下载器代理
		/// </summary>
		/// <param name="allotDownloaderMessage">分配下载器代理的选项</param>
		/// <returns></returns>
		protected virtual async Task<bool> AllocateAsync(AllotDownloaderMessage allotDownloaderMessage)
		{
			var agents = _agents.Values;
			if (agents.Count == 0)
			{
				Logger.LogInformation($"任务 {allotDownloaderMessage.OwnerId} 未找到活跃的下载器代理");
				return false;
			}

			// TODO: 实现分配
			var allocatedAgents = new[] {agents.First()};
			var agentIds = allocatedAgents.Select(x => x.Id).ToArray();
			// 保存节点选取信息
			await DownloaderAgentStore.AllocateAsync(allotDownloaderMessage.OwnerId, agentIds);
			// 发送消息让下载代理器分配好下载器
			var message =
				$"|{Framework.AllocateDownloaderCommand}|{JsonConvert.SerializeObject(allotDownloaderMessage)}";
			foreach (var agent in agents)
			{
				await Mq.PublishAsync(agent.Id, message);
			}

			Logger.LogInformation(
				$"任务 {allotDownloaderMessage.OwnerId} 分配下载代理器成功: {JsonConvert.SerializeObject(allocatedAgents)}");
			return true;
		}

		/// <summary>
		/// 把下载请求推送到下载器代理
		/// </summary>
		/// <param name="ownerId">任务标识</param>
		/// <param name="requests">请求</param>
		/// <returns></returns>
		protected virtual async Task EnqueueRequests(string ownerId, IEnumerable<Request> requests)
		{
			// 1. 本机下载中心只会有一个下载代理
			// 2. TODO: 如果下载器代理下线如何解决
			// 3. TODO: 实现分配策略
			_allocatedAgents.AddOrUpdate(ownerId,
				new ConcurrentDictionary<string, object>((await DownloaderAgentStore.GetAllocatedListAsync(ownerId))
					.Select(x => x.AgentId).ToDictionary(x => x, x => new object())),
				(s, list) => list);
			var agentIds = _allocatedAgents[ownerId].Keys;
			if (agentIds.Count <= 0)
			{
				Logger.LogError($"任务 {ownerId} 未找到活跃的下载器代理");
			}

			foreach (var agentId in agentIds)
			{
				if (_agents.ContainsKey(agentId))
				{
					var agent = _agents[agentId];
					if ((DateTime.Now - _agents[agentId].LastModificationTime).TotalSeconds < 12)
					{
						var json = JsonConvert.SerializeObject(requests);
						var message = $"|{Framework.DownloadCommand}|{json}";
						await Mq.PublishAsync(agent.Id, message);
					}
					// 遇到下线的节点需要跳过等上线
					else
					{
						 
					}
				}
			}
		}
		
		private Task StartSyncDownloaderAgentDataService()
		{
			return Task.Factory.StartNew(async () =>
			{
				while (_isRunning)
				{
					Thread.Sleep(30000);

					await SyncDownloaderAgentData();
				}
			});
		}

		private async Task SyncDownloaderAgentData()
		{
			var agents = await DownloaderAgentStore.GetAllListAsync();

			foreach (var agent in agents)
			{
				_agents.AddOrUpdate(agent.Id, x => agent, (s, downloaderAgent) =>
				{
					if (downloaderAgent.LastModificationTime < agent.LastModificationTime)
					{
						downloaderAgent.LastModificationTime = agent.LastModificationTime;
					}

					return downloaderAgent;
				});
			}
		}
	}
}