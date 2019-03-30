using System.Collections.Generic;
using DotnetSpider.Downloader;

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
        /// <param name="ownerId">爬虫标识</param>
        /// <param name="count">出队数</param>
        /// <returns>请求</returns>
        Request[] Dequeue(string ownerId, int count = 1);

        /// <summary>
        /// 请求入队
        /// </summary>
        /// <param name="requests">请求</param>
        /// <returns>入队个数</returns>
        int Enqueue(IEnumerable<Request> requests);

        /// <summary>
        /// 队列中的总请求个数
        /// </summary>
        int Total { get; }
    }
}