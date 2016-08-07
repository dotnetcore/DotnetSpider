using Java2Dotnet.Spider.Core.Scheduler;
using System;
namespace Java2Dotnet.Spider.Core.Monitor
{
	public interface IMonitorService : IDisposable
	{
		void Watch(SpiderStatus spider);
		bool IsEnabled { get; }
	}
}
