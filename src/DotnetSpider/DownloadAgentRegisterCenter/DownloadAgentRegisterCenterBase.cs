using System;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Common;
using DotnetSpider.DownloadAgentRegisterCenter.Entity;
using DotnetSpider.EventBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotnetSpider.DownloadAgentRegisterCenter
{
	/// <summary>
	/// 下载中心
	/// </summary>
	public abstract class DownloadAgentRegisterCenterBase : BackgroundService, IDownloadAgentRegisterCenter
	{
		/// <summary>
		/// 消息队列
		/// </summary>
		protected readonly IEventBus EventBus;

		/// <summary>
		/// 系统选项
		/// </summary>
		protected readonly SpiderOptions Options;

		/// <summary>
		/// 日志接口
		/// </summary>
		protected readonly ILogger Logger;

		/// <summary>
		/// 下载器代理存储
		/// </summary>
		protected readonly IDownloaderAgentStore DownloaderAgentStore;

		public bool IsRunning { get; private set; }

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
			SpiderOptions options,
			ILogger logger)
		{
			EventBus = eventBus;
			DownloaderAgentStore = downloaderAgentStore;
			Logger = logger;
			Options = options;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				await DownloaderAgentStore.EnsureDatabaseAndTableCreatedAsync();
			}
			catch (Exception e)
			{
				Logger.LogError($"初始化注册中心数据库失败: {e}");
			}

			EventBus.Subscribe(Options.TopicDownloaderAgentRegisterCenter, async message =>
			{
				if (message == null)
				{
					Logger.LogWarning("接收到空消息");
					return;
				}

				if (message.IsTimeout(60))
				{
					Logger.LogWarning($"消息超时: {JsonConvert.SerializeObject(message)}");
					return;
				}

				switch (message.Type)
				{
					case Framework.RegisterCommand:
					{
						// 此处不考虑消息的超时，一是因为节点数量不会很多，二是因为超时的可以释放掉
						var agent = JsonConvert.DeserializeObject<DownloaderAgent>(message.Data);
						if (agent != null)
						{
							await DownloaderAgentStore.RegisterAsync(agent);
							Logger.LogInformation($"注册下载代理器 {agent.Id} 成功");
						}
						else
						{
							Logger.LogError($"注册下载代理器消息不正确: {message.Data}");
						}

						break;
					}

					case Framework.HeartbeatCommand:
					{
						var heartbeat = JsonConvert.DeserializeObject<DownloaderAgentHeartbeat>(message.Data);
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
							Logger.LogError($"下载代理器心跳信息不正确: {message.Data}");
						}

						break;
					}
				}
			});
			Logger.LogInformation("下载中心启动完毕");
			IsRunning = true;
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			EventBus.Unsubscribe(Options.TopicDownloaderAgentRegisterCenter);
			Logger.LogInformation("下载中心退出");
			return base.StopAsync(cancellationToken);
		}
	}
}