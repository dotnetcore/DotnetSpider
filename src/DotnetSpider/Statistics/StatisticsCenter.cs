using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.MessageQueue;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Statistics
{
    /// <summary>
    /// 统计服务中心
    /// </summary>
    public class StatisticsCenter : IStatisticsCenter
    {
        private bool _isRunning;

        private readonly IMessageQueue _mq;
        private readonly ILogger _logger;
        private readonly IStatisticsStore _statisticsStore;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="mq">消息队列接口</param>
        /// <param name="statisticsStore">统计存储接口</param>
        /// <param name="logger">日志接口</param>
        public StatisticsCenter(IMessageQueue mq, IStatisticsStore statisticsStore,
            ILogger<StatisticsCenter> logger)
        {
            _mq = mq;
            _statisticsStore = statisticsStore;
            _logger = logger;
        }

        /// <summary>
        /// 启动统计服务中心
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="SpiderException"></exception>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_isRunning)
            {
                throw new SpiderException("统计中心正在运行中");
            }

            _logger.LogInformation("统计中心启动");

            _mq.Subscribe(Framework.StatisticsServiceTopic, async message =>
            {
                var commandMessage = message.ToCommandMessage();
                if (commandMessage == null)
                {
                    _logger.LogWarning($"接收到非法消息: {message}");
                    return;
                }

                switch (commandMessage.Command)
                {
                    case "Success":
                    {
                        var ownerId = commandMessage.Message;
                        await _statisticsStore.IncrementSuccessAsync(ownerId);
                        break;
                    }
                    case "Failed":
                    {
                        var data = commandMessage.Message.Split(',');
                        await _statisticsStore.IncrementFailedAsync(data[0], int.Parse(data[1]));
                        break;
                    }
                    case "Start":
                    {
                        var ownerId = commandMessage.Message;
                        await _statisticsStore.StartAsync(ownerId);
                        break;
                    }
                    case "Exit":
                    {
                        var ownerId = commandMessage.Message;
                        await _statisticsStore.ExitAsync(ownerId);
                        break;
                    }
                    case "Total":
                    {
                        var data = commandMessage.Message.Split(',');
                        await _statisticsStore.IncrementTotalAsync(data[0], int.Parse(data[1]));

                        break;
                    }
                    case "DownloadSuccess":
                    {
                        var data = commandMessage.Message.Split(',');
                        await _statisticsStore.IncrementDownloadSuccessAsync(data[0], int.Parse(data[1]),
                            long.Parse(data[2]));
                        break;
                    }
                    case "DownloadFailed":
                    {
                        var data = commandMessage.Message.Split(',');
                        await _statisticsStore.IncrementDownloadFailedAsync(data[0], int.Parse(data[1]),
                            long.Parse(data[2]));
                        break;
                    }
                    case "Print":
                    {
                        var ownerId = commandMessage.Message;
                        var statistics = await _statisticsStore.GetSpiderStatisticsAsync(ownerId);
                        _logger.LogInformation(
                            $"任务 {ownerId} 总计 {statistics.Total}, 成功 {statistics.Success}, 失败 {statistics.Failed}, 剩余 {statistics.Total - statistics.Success - statistics.Failed}");
                        break;
                    }
                }
            });
#if NETFRAMEWORK
            return DotnetSpider.Core.Framework.CompletedTask;
#else
            return Task.CompletedTask;
#endif
        }

        /// <summary>
        /// 停止统计中心
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _mq.Unsubscribe(Framework.StatisticsServiceTopic);
            _isRunning = false;
            _logger.LogInformation("统计中心退出");
#if NETFRAMEWORK
            return DotnetSpider.Core.Framework.CompletedTask;
#else
            return Task.CompletedTask;
#endif
        }
    }
}