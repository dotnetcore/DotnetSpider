using System.Collections.Generic;

namespace Java2Dotnet.Spider.Extension.Scheduler
{
	public interface ISchedulerManager
	{
		IDictionary<string, double> GetTaskList(int startIndex, int count);

		void RemoveTask(string taskIdentify);

		SpiderStatus GetTaskStatus(string taskIdentify);
	}
}
