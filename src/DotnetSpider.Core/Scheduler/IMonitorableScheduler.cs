namespace DotnetSpider.Core.Scheduler
{
	/// <summary>
	/// The scheduler whose requests can be counted for monitor.
	/// </summary>
	public interface IMonitorable
	{
		bool IsExited { get; set; }

		long LeftRequestsCount { get; }

		long TotalRequestsCount { get; }

		long SuccessRequestsCount { get; }

		long ErrorRequestsCount { get; }

		void IncreaseSuccessCount();

		void IncreaseErrorCount();
	}
}