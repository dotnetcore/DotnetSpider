using System.Runtime.CompilerServices;

namespace Java2Dotnet.Spider.Core.Scheduler 
{
	/// <summary>
	/// The scheduler whose requests can be counted for monitor.
	/// </summary>
	public interface IMonitorableScheduler : IScheduler
	{
		int GetLeftRequestsCount(ISpider spider);

		int GetTotalRequestsCount(ISpider spider);
	}
}