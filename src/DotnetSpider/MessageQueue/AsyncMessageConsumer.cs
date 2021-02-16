using System;
using System.Threading.Tasks;

namespace DotnetSpider.MessageQueue
{
	public class AsyncMessageConsumer : AsyncMessageConsumer<object>
	{
		public AsyncMessageConsumer(string queue) : base(queue)
		{
		}
	}

	public class AsyncMessageConsumer<TMessage>
	{
		public bool Registered { get; private set; }

		public string Queue { get; }

		public event AsyncMessageHandler<TMessage> Received;

		public event Action<AsyncMessageConsumer<TMessage>> OnClosing;

		public AsyncMessageConsumer(string queue)
		{
			if (string.IsNullOrWhiteSpace(queue))
			{
				throw new ArgumentNullException(nameof(queue));
			}

			Queue = queue;
		}

		public void Register()
		{
			Registered = true;
		}

		public async Task InvokeAsync(TMessage message)
		{
			if (Received == null)
			{
				throw new ArgumentException("Received delegate is null");
			}

			await Received(message);
		}

		public virtual void Close()
		{
			OnClosing?.Invoke(this);
		}
	}
}
