using System;
using System.Linq;
using DotnetSpider.Core;

namespace DotnetSpider.MessageQueue
{
    public static class MessageQueueExtensions
    {
        /// <summary>
        /// 把消息转化成 `命令消息` 对象
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns>命令消息</returns>
        public static CommandMessage ToCommandMessage(this string message)
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
                ? new CommandMessage
                {
                    Command = message.Substring(1, commandEndAt - 1),
                    Message = message.Substring(commandEndAt + 1)
                }
                : null;
        }
    }
}