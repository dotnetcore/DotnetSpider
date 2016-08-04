using Java2Dotnet.Spider.Core.Scheduler;
using Java2Dotnet.Spider.Ioc;
using System;
namespace Java2Dotnet.Spider.Core.Monitor
{
	public interface IMonitorService : IService, IDisposable
	{
		void Watch(SpiderStatus spider);
		bool IsEnable { get; }
	}
}
