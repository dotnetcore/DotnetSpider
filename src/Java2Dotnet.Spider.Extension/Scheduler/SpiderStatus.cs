using System;

namespace Java2Dotnet.Spider.Extension.Scheduler
{
	public class SpiderStatus
	{
		public string Name { get; set; }
		public string Status { get; set; }
		public int AliveThreadCount { get; set; }
		public int ThreadCount { get; set; }
		public long TotalPageCount { get; set; }
		public long LeftPageCount { get; set; }
		public long SuccessPageCount { get; set; }
		public long ErrorPageCount { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public double PagePerSecond { get; set; }
	}
}
