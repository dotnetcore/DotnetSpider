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
    internal abstract class DownloaderAgentBase : IDownloaderAgent
    {
        private bool _isRunning;

        private readonly IMessageQueue _mq;
        private readonly IDownloaderAllocator _downloaderAllocator;
        private readonly IDownloaderAgentOptions _options;

        private readonly ConcurrentDictionary<string, IDownloader> _cache =
            new ConcurrentDictionary<string, IDownloader>();

        /// <summary>
        /// 日志接口
        /// </summary>
        protected ILogger Logger { get; }

        protected Action<IDownloader> ConfigureDownloader { get; set; }

        protected DownloaderAgentBase(
            IDownloaderAgentOptions options,
            IMessageQueue mq,
            IDownloaderAllocator downloaderAllocator,
            ILoggerFactory loggerFactory)
        {
            _mq = mq;
            _downloaderAllocator = downloaderAllocator;
            _options = options;
            Logger = loggerFactory.CreateLogger(GetType().FullName);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_isRunning)
            {
                throw new SpiderException("下载器代理正在运行中");
            }

            _isRunning = true;

            // 注册节点
            var json = JsonConvert.SerializeObject(new DownloaderAgent
            {
                Id = _options.AgentId,
                Name = _options.Name,
                ProcessorCount = Environment.ProcessorCount,
                TotalMemory = Framework.TotalMemory
            });
            await _mq.PublishAsync(Framework.DownloaderCenterTopic, $"|{Framework.RegisterCommand}|{json}");

            // 订阅节点编号
            _mq.Subscribe(_options.AgentId, async message =>
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    Logger.LogWarning("下载器代理接收到空消息");
                    return;
                }
#if DEBUG

                Logger.LogDebug($"下载器代理接收到消息: {message}");
#endif

                try
                {
                    var commandMessage = message.ToCommandMessage();

                    if (commandMessage == null)
                    {
                        Logger.LogWarning($"下载器代理接收到非法消息: {message}");
                        return;
                    }

                    switch (commandMessage.Command)
                    {
                        case Framework.AllocateDownloaderCommand:
                        {
                            await AllotDownloaderAsync(commandMessage.Message);
                            break;
                        }
                        case Framework.DownloadCommand:
                        {
                            await DownloadAsync(commandMessage.Message);
                            break;
                        }
                        case Framework.ExitCommand:
                        {
                            await StopAsync(default);
                            break;
                        }
                        default:
                        {
                            Logger.LogError($"下载器代理无法处理消息: {message}");
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError($"下载器代理处理消息: {message} 失败, 异常: {e}");
                }
            });

            // 开始心跳
            HeartbeatAsync(cancellationToken).ConfigureAwait(false).GetAwaiter();

            // 循环清理过期下载器
            ReleaseDownloaderAsync().ConfigureAwait(false).GetAwaiter();

            Logger.LogInformation("下载器代理启动完毕");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _mq.Unsubscribe(_options.AgentId);
            _isRunning = false;
            Logger.LogInformation("下载器代理退出");
#if NETFRAMEWORK
            return DotnetSpider.Core.Framework.CompletedTask;
#else
            return Task.CompletedTask;
#endif
        }

        private Task HeartbeatAsync(CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(async () =>
            {
                while (_isRunning)
                {
                    Thread.Sleep(5000);

                    var json = JsonConvert.SerializeObject(new DownloaderAgentHeartbeat
                    {
                        Id = _options.AgentId,
                        Name = _options.Name,
                        FreeMemory = (int) Framework.GetFreeMemory(),
                        DownloaderCount = _cache.Count
                    });
                    await _mq.PublishAsync(Framework.DownloaderCenterTopic,
                        $"|{Framework.HeartbeatCommand}|{json}");
                }
            }, cancellationToken);
        }

        private Task DownloadAsync(string message)
        {
            var requests = JsonConvert.DeserializeObject<Request[]>(message);
            if (requests.Length > 0)
            {
                // 下载中心下载请求批量传送，因此反序列化的请求需要按拥有者标号分组。
                // 对于同一个任务应该是顺序下载。TODO: 因为是使用多线程，是否此时保证顺序并不会启作用？
                var groupings = requests.GroupBy(x => x.OwnerId).ToDictionary(x => x.Key, y => y.ToList());
                foreach (var grouping in groupings)
                {
                    foreach (var request in grouping.Value)
                    {
                        Task.Factory.StartNew(async () =>
                        {
                            var response = await DownloadAsync(request);
                            if (response != null)
                            {
                                await _mq.PublishAsync($"{Framework.ResponseHandlerTopic}{grouping.Key}",
                                    JsonConvert.SerializeObject(new[] {response}));
                            }
                        }).ConfigureAwait(false).GetAwaiter();
                    }
                }
            }
            else
            {
                Logger.LogWarning("下载请求数: 0");
            }

#if NETFRAMEWORK
            return DotnetSpider.Core.Framework.CompletedTask;
#else
            return Task.CompletedTask;
#endif
        }

        private async Task<Response> DownloadAsync(Request request)
        {
            if (_cache.TryGetValue(request.OwnerId, out IDownloader downloader))
            {
                var response = await downloader.DownloadAsync(request);
                return response;
            }

            var msg = $"未找到任务 {request.OwnerId} 的下载器";
            Logger.LogError(msg);
            return new Response
            {
                Request = request,
                Exception = msg,
                Success = false,
                AgentId = _options.AgentId
            };
        }

        private Task ReleaseDownloaderAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                while (_isRunning)
                {
                    Thread.Sleep(1000);

                    try
                    {
                        var now = DateTime.Now;
                        var expires = new List<string>();
                        foreach (var kv in _cache)
                        {
                            var downloader = kv.Value;
                            if ((now - downloader.LastUsedTime).TotalSeconds > 300)
                            {
                                downloader.Dispose();
                                expires.Add(kv.Key);
                            }
                        }

                        foreach (var expire in expires)
                        {
                            _cache.TryRemove(expire, out _);
                        }

                        if (expires.Count > 0)
                        {
                            Logger.LogInformation($"释放过期下载器: {expires.Count}");
                        }
                        else
                        {
                            Logger.LogDebug($"释放过期下载器: {expires.Count}");
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogDebug($"释放过期下载器失败: {e}");
                    }
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task AllotDownloaderAsync(string message)
        {
            var allotDownloaderMessage = JsonConvert.DeserializeObject<AllotDownloaderMessage>(message);
            if (!_cache.ContainsKey(allotDownloaderMessage.OwnerId))
            {
                var downloaderEntry =
                    await _downloaderAllocator.CreateDownloaderAsync(_options.AgentId, allotDownloaderMessage);

                if (downloaderEntry == null)
                {
                    Logger.LogError($"任务 {allotDownloaderMessage.OwnerId} 分配下载器 {allotDownloaderMessage.Type} 失败");
                }
                else
                {
                    ConfigureDownloader?.Invoke(downloaderEntry);
                    _cache.TryAdd(allotDownloaderMessage.OwnerId, downloaderEntry);
                }
            }
            else
            {
                Logger.LogWarning($"任务 {allotDownloaderMessage.OwnerId} 重复分配下载器");
            }
        }
    }
}