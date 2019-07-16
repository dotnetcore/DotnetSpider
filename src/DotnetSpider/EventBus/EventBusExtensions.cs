namespace DotnetSpider.EventBus
{
	public static class EventBusExtensions
	{
//		/// <summary>
//		/// 把消息转化成 `命令消息` 对象
//		/// </summary>
//		/// <param name="message">消息</param>
//		/// <returns>命令消息</returns>
//		public static Event ToEvent(this string message)
//		{
//			if (string.IsNullOrWhiteSpace(message))
//			{
//				return null;
//			}
//
//			if ('|' != message.ElementAtOrDefault(0))
//			{
//				return null;
//			}
//
//			var timestampEndAt = message.IndexOf(Framework.CommandSeparator, 1, StringComparison.Ordinal);
//			var timestamp = message.Substring(1, timestampEndAt - 1);
//
//			var nameEndAt = message.IndexOf(Framework.CommandSeparator, timestampEndAt + 1, StringComparison.Ordinal);
//			var name = message.Substring(timestampEndAt + 1, nameEndAt - timestampEndAt - 1);
//			var msg = message.Substring(nameEndAt + 1, message.Length - nameEndAt);
//
//			return new Event
//			{
//				Type = name,
//				Timestamp = DateTimeHelper.ToDateTimeOffset(long.Parse(timestamp)),
//				Message = msg
//			};
//		}

	}
}