using DotnetSpider.Core.Scheduler;
using System;
namespace DotnetSpider.Core.Monitor
{
	public interface IMonitorService : IDisposable
	{
		void Watch(SpiderStatus spider);
		bool IsEnabled { get; }
	}
}
