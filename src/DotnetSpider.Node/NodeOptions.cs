using System;

namespace DotnetSpider.Node
{
	public class NodeOptions
	{
		public string Broker { get; set; }
		public string Group { get; set; }
		public int Heartbeat { get; set; }
		public int? ProcessCount { get; set; } = Environment.ProcessorCount * 2;
		public string Token { get; set; }
		public int RetryDownload { get; set; }
		public int RertyPush { get; set; } = 3600;
	}
}
