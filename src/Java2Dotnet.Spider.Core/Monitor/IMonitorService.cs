using Java2Dotnet.Spider.Core.Scheduler;
using Java2Dotnet.Spider.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Java2Dotnet.Spider.Core.Monitor
{
	public interface IMonitorService : IService, IDisposable
	{
		void SaveStatus(dynamic spider);
		bool IsValid { get; }
	}
}
