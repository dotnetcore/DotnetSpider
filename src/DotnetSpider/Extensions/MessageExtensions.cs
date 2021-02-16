using System;

namespace DotnetSpider.Extensions
{
	public static class MessageExtensions
	{
		public static bool WhetherTimeout(this MessageQueue.Message message, int seconds = 30)
		{
			var dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(message.Timestamp);
			return (DateTimeOffset.Now - dateTimeOffset).TotalSeconds < seconds;
		}
	}
}
