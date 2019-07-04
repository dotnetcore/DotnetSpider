using System;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.DownloadAgentRegisterCenter.Entity;
using DotnetSpider.EventBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotnetSpider.DownloadAgentRegisterCenter
{
	/// <summary>
	/// 下载中心
	/// </summary>
	public abstract class DownloadAgentRegisterCenterBase : IDownloadAgentRegisterCenter
	{
		public bool IsRunning { get; private set; }
		
		/// <summary>
		/// 消息队列
		/// </summary>
		protected readonly IEventBus EventBus;

		/// <summary>
		/// 系统选项
		/// </summary>
		protected readonly ISpiderOptions Options;

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
		/// <param name="eventBus">消息队列</param>
		/// <param name="downloaderAgentStore">下载器代理存储</param>
		/// <param name="options">系统选项</param>
		/// <param name="logger">日志接口</param>
		protected DownloadAgentRegisterCenterBase(
			IEventBus eventBus,
			IDownloaderAgentStore downloaderAgentStore,
			ISpiderOptions options,
			ILogger logger)
		{
			EventBus = eventBus;
			DownloaderAgentStore = downloaderAgentStore;
			Logger = logger;
			Options = options;
		}

		/// <summary>
		/// 启动下载中心
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <exception cref="SpiderException"></exception>
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			if (IsRunning)
			{
				throw new SpiderException("下载中心正在运行中");
			}

			await DownloaderAgentStore.EnsureDatabaseAndTableCreatedAsync();

			EventBus.Subscribe(Framework.DownloaderAgentRegisterCenterTopic, async message =>
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
						// 此处不考虑消息的超时，一是因为节点数量不会很多，二是因为超时的可以释放掉
						var agent = JsonConvert.DeserializeObject<DownloaderAgent>(commandMessage.Message);
						if (agent != null)
						{
							await DownloaderAgentStore.RegisterAsync(agent);
							Logger.LogInformation($"注册下载代理器 {agent.Id} 成功");
						}
						else
						{
							Logger.LogError($"注册下载代理器消息不正确: {commandMessage.Message}");
						}

						break;
					}
					case Framework.HeartbeatCommand:
					{
						var heartbeat = JsonConvert.DeserializeObject<DownloaderAgentHeartbeat>(commandMessage.Message);
						if (heartbeat != null)
						{
							if ((DateTime.Now - heartbeat.CreationTime).TotalSeconds < Options.MessageExpiredTime)
							{
								await DownloaderAgentStore.HeartbeatAsync(heartbeat);
								Logger.LogDebug($"下载器代理 {heartbeat.AgentId} 更新心跳成功");
							}
							else
							{
								Logger.LogWarning($"下载器代理 {heartbeat.AgentId} 更新心跳过期");
							}
						}
						else
						{
							Logger.LogError($"下载代理器心跳信息不正确: {commandMessage.Message}");
						}

						break;
					}
				}
			});

			IsRunning = true;
			
			Logger.LogInformation("下载中心启动完毕");
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			EventBus.Unsubscribe(Framework.DownloaderAgentRegisterCenterTopic);
			IsRunning = false;
			Logger.LogInformation("下载中心退出");
			return Task.CompletedTask;
		}
	}
}