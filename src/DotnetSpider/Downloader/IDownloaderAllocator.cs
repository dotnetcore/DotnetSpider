using System.Threading.Tasks;
using DotnetSpider.Downloader.Entity;

namespace DotnetSpider.Downloader
{
    /// <summary>
    /// 分配下载器的接口
    /// </summary>
    public interface IDownloaderAllocator
    {
        /// <summary>
        /// 创建下载器
        /// </summary>
        /// <param name="agentId"></param>
        /// <param name="allotDownloaderMessage"></param>
        /// <returns></returns>
        Task<IDownloader> CreateDownloaderAsync(string agentId,
            AllotDownloaderMessage allotDownloaderMessage);
    }
}