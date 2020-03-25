using System;
using System.Threading.Tasks;

namespace SwiftMQ
{
	public class AsyncConsumer
	{
		public event AsyncEventHandler<object> Received;

		public AsyncConsumer(string queue)
		{
			if (string.IsNullOrWhiteSpace(queue))
			{
				throw new ArgumentNullException(nameof(queue));
			}

			Queue = queue;
		}

		public string Queue { get; private set; }

		public async Task InvokeAsync(object message)
		{
			if (Received == null)
			{
				throw new ArgumentException("Received delegate is null");
			}

			if (Received.Invoke(message) is { } task)
			{
				await task;
			}
		}
	}
}
