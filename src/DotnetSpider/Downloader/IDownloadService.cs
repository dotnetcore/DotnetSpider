using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetSpider.Downloader.Entity;

namespace DotnetSpider.Downloader
{
    public interface IDownloadService
    {
        /// <summary>
        /// 请求分配下载代理器
        /// </summary>
        /// <param name="allotDownloaderMessage"></param>
        /// <returns></returns>
        Task<bool> AllocateAsync(AllotDownloaderMessage allotDownloaderMessage);

        /// <summary>
        /// TODO: 根据策略分配下载器: 1. Request 从哪个下载器返回的需要返回到对应的下载器  2. 随机一个下载器
        /// </summary>
        /// <param name="ownerId">爬虫标识</param>
        /// <param name="requests">请求</param>
        /// <returns></returns>
        Task EnqueueRequests(string ownerId, IEnumerable<Request> requests);
    }
}