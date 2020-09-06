namespace DotnetSpider.Message.Statistics
{
	public class Total : MessageQueue.Message
	{
		public string SpiderId { get; set; }
		public long Count { get; set; }

		public Total()
		{
		}

		public Total(string spiderId, long count)
		{
			SpiderId = spiderId;
			Count = count;
		}
	}
}
