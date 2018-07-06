namespace DotnetSpider.Core.Scheduler
{
	/// <summary>
	/// The scheduler whose requests can be counted for monitor.
	/// </summary>
	public interface IMonitorable
	{
		/// <summary>
		/// 剩余链接数
		/// </summary>
		long LeftRequestsCount { get; }

		/// <summary>
		/// 总的链接数
		/// </summary>
		long TotalRequestsCount{ get; }

		/// <summary>
		/// 采集成功的链接数
		/// </summary>
		long SuccessRequestsCount { get; }

		/// <summary>
		/// 采集失败的次数, 不是链接数, 如果一个链接采集多次都失败会记录多次
		/// </summary>
		long ErrorRequestsCount { get; }

		/// <summary>
		/// 采集成功的链接数加 1
		/// </summary>
		void IncreaseSuccessCount();

		/// <summary>
		/// 采集失败的次数加 1
		/// </summary>
		void IncreaseErrorCount();
	}
}