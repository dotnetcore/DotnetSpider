using System;
using DotnetSpider.Common;

namespace DotnetSpider.MessageQueue
{
	public class MessageData<TData>
	{
		public string Type { get; set; }

		public long Timestamp { get; set; }

		public TData Data { get; set; }

		public bool IsTimeout(int seconds = 30)
		{
			var timestamp = DateTimeHelper.ToUnixTime(Timestamp);
			return (DateTimeOffset.Now - timestamp).TotalSeconds <= seconds;
		}
	}
}
