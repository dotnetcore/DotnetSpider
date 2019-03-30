using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetSpider.Downloader.Entity;

namespace DotnetSpider.Downloader
{
    public interface IDownloaderAgentStore
    {
        /// <summary>
        /// 查询所有已经注册代理，用于分配下载节点
        /// </summary>
        /// <returns></returns>
        Task<List<DownloaderAgent>> GetAllListAsync();

        /// <summary>
        /// 查询任务所分配的下载代理
        /// </summary>
        /// <param name="ownerId"></param>
        /// <returns></returns>
        Task<List<DownloaderAgent>> GetAllListAsync(string ownerId);
        
        /// <summary>
        /// 添加下载代理
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        Task RegisterAsync(DownloaderAgent agent);

        /// <summary>
        /// 保存下载代理的心跳
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        Task HeartbeatAsync(DownloaderAgentHeartbeat agent);
        
        /// <summary>
        /// 给任务分配下载代理
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="agentIds"></param>
        /// <returns></returns>
        Task AllocateAsync(string ownerId, IEnumerable<string> agentIds);
    }
}