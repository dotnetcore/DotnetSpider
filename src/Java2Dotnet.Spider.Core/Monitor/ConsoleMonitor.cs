using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Core.Scheduler;

namespace Java2Dotnet.Spider.Core.Monitor
{
	public class ConsoleMonitor : IMonitorService
	{
		public bool IsEnable
		{
			get
			{
				return true;
			}
		}

		public void Dispose()
		{
		}

		public void Watch(SpiderStatus status)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"Status: Left {status.Left}, Success {status.Success}, Error: {status.Error}, Total {status.Total}, Thread {status.ThreadNum}");
			Console.ResetColor();
		}
	}
}
