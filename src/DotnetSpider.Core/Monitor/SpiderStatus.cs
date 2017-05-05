﻿namespace DotnetSpider.Core.Monitor
{
	public class SpiderStatus
	{
		public string Status { get; set; }
		public long Left { get; set; }
		public long Total { get; set; }
		public long Success { get; set; }
		public long Error { get; set; }
		public long AvgDownloadSpeed { get; set; }
		public long AvgProcessorSpeed { get; set; }
		public long AvgPipelineSpeed { get; set; }
		public string Code { get; set; }
		public int ThreadNum { get; set; }
		public string Identity { get; set; }
		public string Machine { get; set; }
		public string UserId { get; set; }
		public string TaskGroup { get; set; }
		public string Timestamp { get; set; }
	}
}
