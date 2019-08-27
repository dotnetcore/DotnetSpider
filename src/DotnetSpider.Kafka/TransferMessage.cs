namespace DotnetSpider.Kafka
{
	public class TransferMessage
	{
		public string Type { get; set; }

		public long Timestamp { get; set; }

		public byte[] Data { get; set; }
	}
}
