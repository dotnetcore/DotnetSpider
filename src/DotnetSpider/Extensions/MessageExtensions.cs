using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Infrastructure;
using LZ4;
using MessagePack;

namespace DotnetSpider.Extensions
{
    public static class MessageExtensions
    {
        public static bool WhetherTimeout(this Message message, int seconds = 30)
        {
            var dateTimeOffset = message.Timestamp.ToDateTimeOffset();
            return (DateTimeOffset.Now - dateTimeOffset).TotalSeconds < seconds;
        }

        public static byte[] ToBytes(this object message)
        {
            message.NotNull(nameof(message));
            var bytes = MessagePackSerializer.Typeless.Serialize(message);
            return LZ4Codec.Wrap(bytes);
        }

        public static async Task<object> DeserializeAsync(this byte[] bytes, CancellationToken cancellationToken)
        {
	        bytes = LZ4Codec.Unwrap(bytes);
            var stream = new MemoryStream(bytes);
            return await MessagePackSerializer.Typeless.DeserializeAsync(stream, null, cancellationToken);
        }
    }
}
