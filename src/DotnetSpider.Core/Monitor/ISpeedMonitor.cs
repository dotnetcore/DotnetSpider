namespace DotnetSpider.Core.Monitor
{
	/// <summary>
	/// 爬虫速度监控
	/// </summary>
	public interface ISpeedMonitor
	{
		/// <summary>
		/// 平均下载一个链接需要的时间(豪秒)
		/// </summary>
		long AvgDownloadSpeed { get; }
		/// <summary>
		/// 平均解析一个页面需要的时间(豪秒)
		/// </summary>
		long AvgProcessorSpeed { get; }
		/// <summary>
		/// 数据管道处理一次数据结果需要的时间(豪秒)
		/// </summary>
		long AvgPipelineSpeed { get; }
	}
}
