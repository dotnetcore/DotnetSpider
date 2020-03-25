using System;
using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SwiftMQ
{
	public static class SwiftMQ
	{
		private static readonly ConcurrentDictionary<string, Channel<object>> ChannelDict =
			new ConcurrentDictionary<string, Channel<object>>();

		public static bool QueueDeclare(string queue)
		{
			if (!ChannelDict.ContainsKey(queue))
			{
				var channel = Channel.CreateUnbounded<object>();
				return ChannelDict.TryAdd(queue, channel);
			}

			return true;
		}

		public static async Task PublishAsync(string queue, object message)
		{
			if (!ChannelDict.ContainsKey(queue))
			{
				throw new ApplicationException("Use SwiftMQ.QueueDeclare to create a queue firstly");
			}

			await ChannelDict[queue].Writer.WriteAsync(message);
		}

		public static Task ConsumeAsync(AsyncConsumer consumer)
		{
			if (!ChannelDict.ContainsKey(consumer.Queue))
			{
				throw new ApplicationException("Use SwiftMQ.QueueDeclare to create a queue firstly");
			}

			return Task.Factory.StartNew(async () =>
			{
				var channel = ChannelDict[consumer.Queue];
				while (await channel.Reader.WaitToReadAsync())
				{
					var message = await channel.Reader.ReadAsync();
					await consumer.InvokeAsync(message);
				}
			});
		}
	}
}
