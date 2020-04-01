using System.Threading.Tasks;

namespace DotnetSpider.Statistics
{
	public interface IStatisticsClient
	{
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

		Task PrintAsync(string id);
	}
}
