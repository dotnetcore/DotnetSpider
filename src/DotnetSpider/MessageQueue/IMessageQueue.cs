using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpider.MessageQueue
{
	public interface IMessageQueue
	{
		Task PublishAsync(string queue, byte[] message);

		Task ConsumeAsync(AsyncMessageConsumer<byte[]> consumer, CancellationToken cancellationToken);

		void CloseQueue(string queue);
	}
}
