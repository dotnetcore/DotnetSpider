using System.Runtime.CompilerServices;

namespace Java2Dotnet.Spider.Core.Scheduler 
{
	/// <summary>
	/// The scheduler whose requests can be counted for monitor.
	/// </summary>
	public interface IMonitorableScheduler : IScheduler
	{
		long GetLeftRequestsCount();

		long GetTotalRequestsCount();

		long GetSuccessRequestsCount();

		long GetErrorRequestsCount();

		void IncreaseSuccessCounter();

		void IncreaseErrorCounter();
	}
}