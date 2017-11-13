namespace DotnetSpider.Core.Monitor
{
	public class SpiderStatus
	{
		public virtual string TaskId { get; set; }
		public virtual string Identity { get; set; }
		public virtual string NodeId { get; set; }
		public virtual string Status { get; set; }
		public virtual int Thread { get; set; }
		public virtual long Left { get; set; }
		public virtual long Success { get; set; }
		public virtual long Error { get; set; }
		public virtual long Total { get; set; }
		public virtual float AvgDownloadSpeed { get; set; }
		public virtual float AvgProcessorSpeed { get; set; }
		public virtual float AvgPipelineSpeed { get; set; }
	}
}
