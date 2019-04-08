using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetSpider.Downloader.Entity;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 下载器代理存储
	/// </summary>
	public interface IDownloaderAgentStore
	{
		/// <summary>
		/// 创建数据库
		/// </summary>
		/// <returns></returns>
		Task EnsureDatabaseAndTableCreatedAsync();

		/// <summary>
		/// 查询所有已经注册并且最后一次心跳上报时间在当前时间 12 秒以内的下载器代理
		/// </summary>
		/// <returns></returns>
		Task<List<DownloaderAgent>> GetAllListAsync();

		/// <summary>
		/// 查询任务所分配的下载代理
		/// </summary>
		/// <param name="ownerId">任务标识</param>
		/// <returns></returns>
		Task<List<DownloaderAgentAllocate>> GetAllocatedListAsync(string ownerId);

		/// <summary>
		/// 添加下载器代理
		/// </summary>
		/// <param name="agent">下载器代理</param>
		/// <returns></returns>
		Task RegisterAsync(DownloaderAgent agent);

		/// <summary>
		/// 保存下载器代理的心跳
		/// </summary>
		/// <param name="agent">下载器代理</param>
		/// <returns></returns>
		Task HeartbeatAsync(DownloaderAgentHeartbeat agent);

		/// <summary>
		/// 保存给任务分配的下载器代理
		/// </summary>
		/// <param name="ownerId">任务标识</param>
		/// <param name="message">分配下载器代理的消息</param>
		/// <param name="agentIds">分配的下载器代理标识</param>
		/// <returns></returns>
		Task AllocateAsync(string ownerId, string message, IEnumerable<string> agentIds);

		/// <summary>
		/// 获取分配下载器代理的消息
		/// </summary>
		/// <param name="ownerId">任务标识</param>
		/// <returns></returns>
		Task<string> GetAllocateDownloaderMessageAsync(string ownerId);
	}
}