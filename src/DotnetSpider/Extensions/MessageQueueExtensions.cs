using System.Threading.Tasks;
using SwiftMQ;

namespace DotnetSpider.Extensions
{
    public static class MessageQueueExtensions
    {
        public static async Task PublishAsBytesAsync(this IMessageQueue messageQueue, string queue, object message)
        {
            var bytes = message.ToBytes();
            await messageQueue.PublishAsync(queue, bytes);
        }
    }
}