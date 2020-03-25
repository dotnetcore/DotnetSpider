using System;
using System.Threading.Tasks;

namespace DotnetSpider.Infrastructure
{
	public class Consumer<TMessage>
	{
		public string Queue { get; private set; }

		public event Func<TMessage, Task> Received;

		public Action Closed;

		public Consumer(string queue)
		{
			if (string.IsNullOrWhiteSpace(queue))
			{
				throw new ArgumentNullException(nameof(queue));
			}

			Queue = queue;
		}

		public async Task InvokeAsync(TMessage message)
		{
			if (Received == null)
			{
				throw new ArgumentException("Received delegate is null");
			}

			await Received.Invoke(message);
		}

		public void Close()
		{
			Closed?.Invoke();
		}
	}
}
