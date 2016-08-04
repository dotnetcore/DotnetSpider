using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Core.Scheduler;
using System.IO;

namespace Java2Dotnet.Spider.Core.Monitor
{
	public class FileMonitor : IMonitorService
	{
		public bool IsEnable
		{
			get
			{
				return true;
			}
		}

		private string _path;
		private StreamWriter Writer { get; set; }

		public FileMonitor()
		{
#if !NET_CORE
			_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "status.txt");
#else
			_path = Path.Combine(AppContext.BaseDirectory, "status.txt");
#endif
			Writer = new StreamWriter(new FileInfo(_path).OpenWrite());
			Writer.AutoFlush = true;
		}

		public void Dispose()
		{
			Writer.Dispose();
		}

		public void Watch(SpiderStatus status)
		{
			string msg = $"[{status.Timestamp}] [{status.Machine}][{status.UserId}][{status.TaskGroup}] [{status.Identity}] STATUS: Left {status.Left}, Success {status.Success}, Error: {status.Error}, Total {status.Total}, Thread {status.ThreadNum}";
			Writer.WriteLine(msg);
		}
	}
}
