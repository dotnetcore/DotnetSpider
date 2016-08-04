namespace Java2Dotnet.Spider.Core.Monitor
{
	public class SpiderStatus
	{
		public long Left { get; set; }
		public long Total { get; set; }
		public long Success { get; set; }
		public long Error { get; set; }
		public string Code { get; set; }
		public int ThreadNum { get; set; }
		public string Identity { get; set; }
		public string Machine { get; set; }
		public string UserId { get; set; }
		public string TaskGroup { get; set; }
		public string Timestamp { get; set; }
	}
}
