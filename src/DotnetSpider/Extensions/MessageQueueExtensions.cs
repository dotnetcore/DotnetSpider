using System.Threading.Tasks;
using DotnetSpider.MessageQueue;

namespace DotnetSpider.Extensions
{
	public static class MessageQueueExtensions
	{
		public static async Task PublishAsBytesAsync<T>(this IMessageQueue messageQueue, string queue, T message)
		{
			var bytes = message.Serialize();
			await messageQueue.PublishAsync(queue, bytes);
		}
	}
}
