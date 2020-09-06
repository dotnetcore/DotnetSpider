using System;

namespace DotnetSpider.MessageQueue
{
	public abstract class Message
	{
		public long Timestamp { get; set; }
		public Guid MessageId { get; set; }
	}
}
