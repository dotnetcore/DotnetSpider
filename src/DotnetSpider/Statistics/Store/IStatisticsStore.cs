using System.Threading.Tasks;
using DotnetSpider.Infrastructure;

namespace DotnetSpider.Statistics.Store
{
	public interface IStatisticsStore
	{
		/// <summary>
		/// 创建数据库和表
		/// </summary>
		/// <returns></returns>
		Task EnsureDatabaseAndTableCreatedAsync();

		/// <summary>
		/// 总请求数加 1
		/// </summary>
		/// <param name="id">爬虫标识</param>
		/// <param name="count"></param>
		/// <returns></returns>
		Task IncreaseTotalAsync(string id, long count);

		/// <summary>
		/// 成功次数加 1
		/// </summary>
		/// <param name="id">爬虫标识</param>
		/// <returns></returns>
		Task IncreaseSuccessAsync(string id);

		/// <summary>
		/// 失败次数加 1
		/// </summary>
		/// <param name="id">爬虫标识</param>
		/// <returns></returns>
		Task IncreaseFailureAsync(string id);

		/// <summary>
		/// 爬虫启动
		/// </summary>
		/// <param name="id">爬虫标识</param>
		/// <param name="name">爬虫名称</param>
		/// <returns></returns>
		Task StartAsync(string id, string name);

		/// <summary>
		/// 爬虫退出
		/// </summary>
		/// <param name="id">爬虫标识</param>
		/// <returns></returns>
		Task ExitAsync(string id);

		/// <summary>
		/// 注册结点
		/// </summary>
		/// <param name="agentId"></param>
		/// <param name="agentName"></param>
		/// <returns></returns>
		Task RegisterAgentAsync(string agentId, string agentName);

		/// <summary>
		/// 下载成功次数加 1
		/// </summary>
		/// <param name="agentId">下载代理器标识</param>
		/// <param name="elapsedMilliseconds">下载总消耗的时间</param>
		/// <returns></returns>
		Task IncreaseAgentSuccessAsync(string agentId, int elapsedMilliseconds);

		/// <summary>
		/// 下载失败次数加 1
		/// </summary>
		/// <param name="agentId">下载代理器标识</param>
		/// <param name="elapsedMilliseconds">下载总消耗的时间</param>
		/// <returns></returns>
		Task IncreaseAgentFailureAsync(string agentId, int elapsedMilliseconds);

		/// <summary>
		/// 分页查询下载代理器的统计信息
		/// </summary>
		/// <param name="agentId"></param>
		/// <param name="page"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		Task<PagedResult<AgentStatistics>> PagedQueryAgentStatisticsAsync(string agentId, int page, int limit);

		/// <summary>
		/// 查询指定下载代理器的统计信息
		/// </summary>
		/// <param name="agentId">下载代理器标识</param>
		/// <returns></returns>
		Task<AgentStatistics> GetAgentStatisticsAsync(string agentId);

		/// <summary>
		/// 查询指定爬虫的统计信息
		/// </summary>
		/// <param name="id">爬虫标识</param>
		/// <returns></returns>
		Task<SpiderStatistics> GetSpiderStatisticsAsync(string id);

		/// <summary>
		/// 分页查询爬虫的统计信息
		/// </summary>
		/// <param name="keyword"></param>
		/// <param name="page"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		Task<PagedResult<SpiderStatistics>> PagedQuerySpiderStatisticsAsync(string keyword, int page, int size);
	}
}
