using System;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Common;
using DotnetSpider.DownloadAgentRegisterCenter.Entity;
using DotnetSpider.MessageQueue;
using MessagePack;
using MessagePack.Resolvers;
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
		protected readonly IMq Mq;

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

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="eventBus">消息队列</param>
		/// <param name="downloaderAgentStore">下载器代理存储</param>
		/// <param name="options">系统选项</param>
		/// <param name="logger">日志接口</param>
		protected DownloadAgentRegisterCenterBase(
			IMq eventBus,
			IDownloaderAgentStore downloaderAgentStore,
			SpiderOptions options,
			ILogger logger)
		{
			Mq = eventBus;
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
				Logger.LogError($"Initialize register center database failed: {e}");
			}

			Mq.Subscribe<byte[]>(Options.TopicDownloaderAgentRegisterCenter, async message =>
			{
				if (message == null)
				{
					Logger.LogWarning("Receive empty message");
					return;
				}

				if (message.IsTimeout(60))
				{
					Logger.LogWarning($"Message is timeout: {JsonConvert.SerializeObject(message)}");
					return;
				}

				switch (message.Type)
				{
					case Framework.RegisterCommand:
					{
						// 此处不考虑消息的超时，一是因为节点数量不会很多，二是因为超时的可以释放掉
						var agent = LZ4MessagePackSerializer.Deserialize<DownloaderAgent>(message.Data, TypelessContractlessStandardResolver.Instance);
						if (agent != null)
						{
							await DownloaderAgentStore.RegisterAsync(agent);
							Logger.LogInformation($"Register agent {agent.Id} success");
						}
						else
						{
							Logger.LogError($"Register agent message is wrong: {message.Data}");
						}

						break;
					}

					case Framework.HeartbeatCommand:
					{
						var heartbeat = LZ4MessagePackSerializer.Deserialize<DownloaderAgentHeartbeat>(message.Data, TypelessContractlessStandardResolver.Instance);
						if (heartbeat != null)
						{
							var interval = (DateTimeOffset.Now - heartbeat.CreationTime).TotalSeconds;
							if (interval < Options.MessageExpiredTime)
							{
								await DownloaderAgentStore.HeartbeatAsync(heartbeat);
								Logger.LogDebug($"Agent {heartbeat.AgentId} refresh heartbeat success");
							}
							else
							{
								Logger.LogWarning($"Agent {heartbeat.AgentId} receive timeout heartbeat");
							}
						}

						break;
					}
				}
			});
			Logger.LogInformation("Agent register center started");
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			Mq.Unsubscribe(Options.TopicDownloaderAgentRegisterCenter);
			Logger.LogInformation("Agent register center exited");
			return base.StopAsync(cancellationToken);
		}
	}
}
