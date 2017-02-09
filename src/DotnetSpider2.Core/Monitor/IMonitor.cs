using System;
namespace DotnetSpider.Core.Monitor
{
	public interface IMonitor : IDisposable
	{
		void Report(SpiderStatus spider);
		bool IsEnabled { get; }
	}
}
