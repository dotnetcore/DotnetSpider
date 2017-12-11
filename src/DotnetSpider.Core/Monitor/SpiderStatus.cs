namespace DotnetSpider.Core.Monitor
{
	public struct SpiderStatus
	{
		public string TaskId { get; set; }
		public string Identity { get; set; }
		public string NodeId { get; set; }
		public string Status { get; set; }
		public int Thread { get; set; }
		public long Left { get; set; }
		public long Success { get; set; }
		public long Error { get; set; }
		public long Total { get; set; }
		public float AvgDownloadSpeed { get; set; }
		public float AvgProcessorSpeed { get; set; }
		public float AvgPipelineSpeed { get; set; }
	}
}
