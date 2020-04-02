using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetSpider.Http;

namespace DotnetSpider.Scheduler
{
    /// <summary>
    /// 调度器接口
    /// </summary>
    public interface IScheduler
    {
        /// <summary>
        /// 从队列中取出指定爬虫的指定个数请求
        /// </summary>
        /// <param name="count">出队数</param>
        /// <returns>请求</returns>
        Task<IEnumerable<Request>> DequeueAsync(int count = 1);

        /// <summary>
        /// 请求入队
        /// </summary>
        /// <param name="requests">请求</param>
        /// <returns>入队个数</returns>
        Task<int> EnqueueAsync(IEnumerable<Request> requests);

        /// <summary>
        /// 队列中的总请求个数
        /// </summary>
        long Total { get; }
    }
}