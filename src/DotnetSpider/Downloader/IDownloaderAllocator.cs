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
		/// <param name="agentId">下载器代理标识</param>
		/// <param name="allotDownloaderMessage">下载器配置信息</param>
		/// <returns></returns>
		Task<IDownloader> CreateDownloaderAsync(string agentId,
			AllotDownloaderMessage allotDownloaderMessage);
	}
}