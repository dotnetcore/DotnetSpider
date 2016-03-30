using System;

namespace Java2Dotnet.Spider.ScriptsConsole
{
	public class NodeInfo
	{
		public string Name { get; set; }
		public int CpuLoad { get; set; }
		public int FreeMemory { get; set; }
		public int TotalMemory { get; set; }
		public string IpAddress { get; set; }
		public DateTime Timestamp { get; set; }
	}
}
