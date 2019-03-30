using System.Threading.Tasks;

namespace DotnetSpider.Statistics
{
    /// <summary>
    /// 统计服务接口
    /// </summary>
    public interface IStatisticsService
    {
        /// <summary>
        /// 增加成功次数 1
        /// </summary>
        /// <param name="ownerId">爬虫标识</param>
        /// <returns></returns>
        Task IncrementSuccessAsync(string ownerId);

        /// <summary>
        /// 添加指定失败次数
        /// </summary>
        /// <param name="ownerId">爬虫标识</param>
        /// <param name="count">失败次数</param>
        /// <returns></returns>
        Task IncrementFailedAsync(string ownerId, int count = 1);

        /// <summary>
        /// 添加总请求数
        /// </summary>
        /// <param name="ownerId">爬虫标识</param>
        /// <param name="count">请求数</param>
        /// <returns></returns>
        Task IncrementTotalAsync(string ownerId, int count);

        /// <summary>
        /// 设置爬虫启动时间
        /// </summary>
        /// <param name="ownerId">爬虫标识</param>
        /// <returns></returns>
        Task StartAsync(string ownerId);

        /// <summary>
        /// 设置爬虫退出时间
        /// </summary>
        /// <param name="ownerId">爬虫标识</param>
        /// <returns></returns>
        Task ExitAsync(string ownerId);

        /// <summary>
        /// 添加指定下载代理器的下载成功次数
        /// </summary>
        /// <param name="agentId">下载代理器标识</param>
        /// <param name="count">下载成功次数</param>
        /// <param name="elapsedMilliseconds">下载总消耗的时间</param>
        /// <returns></returns>
        Task IncrementDownloadSuccessAsync(string agentId, int count, long elapsedMilliseconds);

        /// <summary>
        /// 添加指定下载代理器的下载失败次数
        /// </summary>
        /// <param name="agentId">下载代理器标识</param>
        /// <param name="count">下载失败次数</param>
        /// <param name="elapsedMilliseconds">下载总消耗的时间</param>
        /// <returns></returns>
        Task IncrementDownloadFailedAsync(string agentId, int count, long elapsedMilliseconds);

        /// <summary>
        /// 打印统计信息(仅限本地爬虫使用)
        /// </summary>
        /// <param name="ownerId">爬虫标识</param>
        /// <returns></returns>
        Task PrintStatisticsAsync(string ownerId);
    }
}