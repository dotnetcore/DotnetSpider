namespace DotnetSpider.Message.Spider
{
	public class Exit : MessageQueue.Message
	{
		public string SpiderId { get; set; }

		public Exit(string spiderId)
		{
			SpiderId = spiderId;
		}
	}
}
