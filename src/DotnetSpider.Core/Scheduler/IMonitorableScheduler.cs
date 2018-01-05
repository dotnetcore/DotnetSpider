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
		long TotalRequestsCount { get; }

		/// <summary>
		/// 采集成功的链接数
		/// </summary>
		long SuccessRequestsCount { get; }

		/// <summary>
		/// 采集失败的链接数
		/// </summary>
		long ErrorRequestsCount { get; }

		/// <summary>
		/// 增加一个采集成功的链接数
		/// </summary>
		void IncreaseSuccessCount();

		/// <summary>
		/// 增加一个采集失败的链接数
		/// </summary>
		void IncreaseErrorCount();
	}
}