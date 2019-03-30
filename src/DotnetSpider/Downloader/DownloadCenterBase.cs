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
    /// 
    /// </summary>
    public abstract class DownloadCenterBase : IDownloadCenter
    {
        private bool _isRunning;

        protected readonly IMessageQueue Mq;
        protected readonly ILogger Logger;
        protected readonly IDownloaderAgentStore DownloaderAgentStore;

        protected DownloadCenterBase(
            IMessageQueue mq,
            IDownloaderAgentStore downloaderAgentStore,
            ILogger logger)
        {
            Mq = mq;
            DownloaderAgentStore = downloaderAgentStore;
            Logger = logger;
        }

        protected virtual async Task<bool> AllocateAsync(AllotDownloaderMessage allotDownloaderMessage)
        {
            List<DownloaderAgent> agents = null;
            for (int i = 0; i < 50; ++i)
            {
                agents = await DownloaderAgentStore.GetAllListAsync();
                if (agents.Count <= 0)
                {
                    Thread.Sleep(100);
                }
                else
                {
                    break;
                }
            }

            if (agents == null)
            {
                Logger.LogError("未找到活跃的下载器代理");
                return false;
            }

            // 保存节点选取信息
            await DownloaderAgentStore.AllocateAsync(allotDownloaderMessage.OwnerId, new[] {agents[0].Id});
            Logger.LogInformation($"分配下载器代理成功 OwnerId {allotDownloaderMessage.OwnerId}, Agent {agents[0].Id}");
            // 发送消息让下载代理器分配好下载器
            var message =
                $"|{Framework.AllocateDownloaderCommand}|{JsonConvert.SerializeObject(allotDownloaderMessage)}";
            foreach (var agent in agents)
            {
                await Mq.PublishAsync(agent.Id, message);
            }

            return true;
        }

        protected virtual async Task EnqueueRequests(string ownerId, IEnumerable<Request> requests)
        {
            // 本机下载中心只会有一个下载代理
            var agents = await DownloaderAgentStore.GetAllListAsync(ownerId);
            if (agents.Count <= 0)
            {
                Logger.LogError("未找到活跃的下载器代理");
            }

            var agent = agents[0];
            var json = JsonConvert.SerializeObject(requests);
            var message = $"|{Framework.DownloadCommand}|{json}";
            await Mq.PublishAsync(agent.Id, message);
        }

        /// <summary>
        /// 启动下载中心
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="SpiderException"></exception>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_isRunning)
            {
                throw new SpiderException("下载中心正在运行中");
            }

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
                        await DownloaderAgentStore.RegisterAsync(agent);
                        Logger.LogInformation($"注册下载代理器 {agent.Id} 成功");
                        break;
                    }
                    case Framework.HeartbeatCommand:
                    {
                        var heartbeat = JsonConvert.DeserializeObject<DownloaderAgentHeartbeat>(commandMessage.Message);
                        await DownloaderAgentStore.HeartbeatAsync(heartbeat);
                        break;
                    }
                    case Framework.AllocateDownloaderCommand:
                    {
                        var options = JsonConvert.DeserializeObject<AllotDownloaderMessage>(commandMessage.Message);
                        await AllocateAsync(options);
                        await Mq.PublishAsync($"{Framework.ResponseHandlerTopic}{options.OwnerId}",
                            $"|{Framework.AllocateDownloaderCommand}|true");
                        break;
                    }
                    case Framework.DownloadCommand:
                    {
                        var requests = JsonConvert.DeserializeObject<Request[]>(commandMessage.Message);
                        if (requests.Length > 0)
                        {
                            var ownerId = requests.First().OwnerId;
                            await EnqueueRequests(ownerId, requests);
                        }

                        break;
                    }
                }
            });
            Logger.LogInformation("下载中心启动完毕");
#if NETFRAMEWORK
            return Framework.CompletedTask;
#else
            return Task.CompletedTask;
#endif
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Mq.Unsubscribe(Framework.DownloaderCenterTopic);
            _isRunning = false;
            Logger.LogInformation("本地下载中心退出");
#if NETFRAMEWORK
            return Framework.CompletedTask;
#else
            return Task.CompletedTask;
#endif
        }
    }
}