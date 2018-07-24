using DotnetSpider.Common;

namespace DotnetSpider.Core.Monitor
{
	/// <summary>
	/// NLog 状态监控, 依据NLog配置上报状态到控制台或者日志文件中
	/// </summary>
	public class LogMonitor : IMonitor
	{
		public ILogger Logger{ get; set; }

		/// <summary>
		/// 上报爬虫状态
		/// </summary>
		/// <param name="identity">唯一标识</param>
		/// <param name="taskId">任务编号</param>
		/// <param name="status">爬虫状态: 运行、暂停、退出、完成</param>
		/// <param name="left">剩余的目标链接数</param>
		/// <param name="total">总的需要采集的链接数</param>
		/// <param name="success">成功采集的链接数</param>
		/// <param name="error">采集出错的链接数</param>
		/// <param name="avgDownloadSpeed">平均下载一个链接需要的时间(豪秒)</param>
		/// <param name="avgProcessorSpeed">平均解析一个页面需要的时间(豪秒)</param>
		/// <param name="avgPipelineSpeed">数据管道处理一次数据结果需要的时间(豪秒)</param>
		/// <param name="threadNum">爬虫线程数</param>
		public virtual void Flush(string identity, string taskId, string status, long left, long total, long success, long error, long avgDownloadSpeed, long avgProcessorSpeed, long avgPipelineSpeed, int threadNum)
		{
			string msg = $"Left {left} Success {success} Error {error} Total {total} Dowload {avgDownloadSpeed} Extract {avgProcessorSpeed} Pipeline {avgPipelineSpeed}";
			Logger.Verbose(msg);
		}
	}
}
