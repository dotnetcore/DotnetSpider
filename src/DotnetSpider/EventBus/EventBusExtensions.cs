using System;
using System.Linq;
using DotnetSpider.Common;

namespace DotnetSpider.EventBus
{
	public static class EventBusExtensions
	{
		/// <summary>
		/// 把消息转化成 `命令消息` 对象
		/// </summary>
		/// <param name="message">消息</param>
		/// <returns>命令消息</returns>
		public static Event ToCommandMessage(this string message)
		{
			if (string.IsNullOrWhiteSpace(message))
			{
				return null;
			}

			if ('|' != message.ElementAtOrDefault(0))
			{
				return null;
			}

			var commandEndAt = message.IndexOf(Framework.CommandSeparator, 1, StringComparison.Ordinal);
			return commandEndAt > 0
				? new Event
				{
					Command = message.Substring(1, commandEndAt - 1),
					Message = message.Substring(commandEndAt + 1)
				}
				: null;
		}
	}
}