using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.MessageQueue;

namespace DotnetSpider.Statistics
{
    /// <summary>
    /// 统计服务
    /// </summary>
    public class StatisticsService : IStatisticsService
    {
        private readonly IMessageQueue _mq;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="mq">消息队列接口</param>
        public StatisticsService(IMessageQueue mq)
        {
            _mq = mq;
        }

        /// <summary>
        /// 增加成功次数 1
        /// </summary>
        /// <param name="ownerId">爬虫标识</param>
        /// <returns></returns>
        public async Task IncrementSuccessAsync(string ownerId)
        {
            await _mq.PublishAsync(Framework.StatisticsServiceTopic, $"|Success|{ownerId}");
        }

        /// <summary>
        /// 添加指定失败次数
        /// </summary>
        /// <param name="ownerId">爬虫标识</param>
        /// <param name="count">失败次数</param>
        /// <returns></returns>
        public async Task IncrementFailedAsync(string ownerId, int count = 1)
        {
            await _mq.PublishAsync(Framework.StatisticsServiceTopic, $"|Failed|{ownerId},{count}");
        }

        /// <summary>
        /// 设置爬虫启动时间
        /// </summary>
        /// <param name="ownerId">爬虫标识</param>
        /// <returns></returns>
        public async Task StartAsync(string ownerId)
        {
            await _mq.PublishAsync(Framework.StatisticsServiceTopic, $"|Start|{ownerId}");
        }

        /// <summary>
        /// 设置爬虫退出时间
        /// </summary>
        /// <param name="ownerId">爬虫标识</param>
        /// <returns></returns>
        public async Task ExitAsync(string ownerId)
        {
            await _mq.PublishAsync(Framework.StatisticsServiceTopic, $"|Exit|{ownerId}");
        }

        /// <summary>
        /// 添加指定下载代理器的下载成功次数
        /// </summary>
        /// <param name="agentId">下载代理器标识</param>
        /// <param name="count">下载成功次数</param>
        /// <param name="elapsedMilliseconds">下载总消耗的时间</param>
        /// <returns></returns>
        public async Task IncrementDownloadSuccessAsync(string agentId, int count, long elapsedMilliseconds)
        {
            await _mq.PublishAsync(Framework.StatisticsServiceTopic,
                $"|DownloadSuccess|{agentId},{count},{elapsedMilliseconds}");
        }

        /// <summary>
        /// 添加指定下载代理器的下载失败次数
        /// </summary>
        /// <param name="agentId">下载代理器标识</param>
        /// <param name="count">下载失败次数</param>
        /// <param name="elapsedMilliseconds">下载总消耗的时间</param>
        /// <returns></returns>
        public async Task IncrementDownloadFailedAsync(string agentId, int count, long elapsedMilliseconds)
        {
            await _mq.PublishAsync(Framework.StatisticsServiceTopic,
                $"|DownloadFailed|{agentId},{count},{elapsedMilliseconds}");
        }

        /// <summary>
        /// 打印统计信息(仅限本地爬虫使用)
        /// </summary>
        /// <param name="ownerId">爬虫标识</param>
        /// <returns></returns>
        public async Task PrintStatisticsAsync(string ownerId)
        {
            await _mq.PublishAsync(Framework.StatisticsServiceTopic,
                $"|Print|{ownerId}");
        }

        /// <summary>
        /// 添加总请求数
        /// </summary>
        /// <param name="ownerId">爬虫标识</param>
        /// <param name="count">请求数</param>
        /// <returns></returns>
        public async Task IncrementTotalAsync(string ownerId, int count)
        {
            await _mq.PublishAsync(Framework.StatisticsServiceTopic,
                $"|Total|{ownerId},{count}");
        }
    }
}