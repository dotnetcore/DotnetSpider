using System;
using DotnetSpider.Infrastructure;
using DotnetSpider.MessageQueue;


namespace DotnetSpider.Extensions
{
	public static class MessageExtensions
	{
		public static bool WhetherTimeout(this MessageQueue.Message message, int seconds = 30)
		{
			var dateTimeOffset = DateTimeHelper.ToDateTimeOffset(message.Timestamp);
			return (DateTimeOffset.Now - dateTimeOffset).TotalSeconds < seconds;
		}
	}
}
