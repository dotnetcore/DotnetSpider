using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DotnetSpider.MessageQueue
{
	public class MessageQueue : IMessageQueue
	{
		private readonly ConcurrentDictionary<string, Channel<byte[]>> _channelDict =
			new ConcurrentDictionary<string, Channel<byte[]>>();

		public async Task PublishAsync(string queue, byte[] message)
		{
			if (!DeclareQueue(queue))
			{
				throw new ApplicationException("Declare queue failed");
			}

			await _channelDict[queue].Writer.WriteAsync(message);
		}

		public async Task ConsumeAsync(AsyncMessageConsumer<byte[]> consumer,
			CancellationToken cancellationToken)
		{
			if (consumer.Registered)
			{
				throw new ApplicationException("This consumer is already registered");
			}

			if (!DeclareQueue(consumer.Queue))
			{
				throw new ApplicationException("Declare queue failed");
			}

			var channel = _channelDict[consumer.Queue];
			consumer.Register();
			consumer.OnClosing += x => { CloseQueue(x.Queue); };

			await Task.Factory.StartNew(async () =>
			{
				while (await channel.Reader.WaitToReadAsync(cancellationToken))
				{
					var bytes = await channel.Reader.ReadAsync(cancellationToken);
					Task.Factory.StartNew(async () =>
						{
							await consumer.InvokeAsync(bytes);
						}, cancellationToken)
						.ConfigureAwait(false).GetAwaiter();
				}
			}, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
		}

		public void CloseQueue(string queue)
		{
			if (_channelDict.TryRemove(queue, out var channel))
			{
				try
				{
					channel.Writer.Complete();
				}
				catch
				{
					// ignore
				}
			}
		}

		private bool DeclareQueue(string queue)
		{
			if (!_channelDict.ContainsKey(queue))
			{
				var channel = Channel.CreateUnbounded<byte[]>();
				return _channelDict.TryAdd(queue, channel);
			}

			return true;
		}

		public void Dispose()
		{
		}
	}
}
