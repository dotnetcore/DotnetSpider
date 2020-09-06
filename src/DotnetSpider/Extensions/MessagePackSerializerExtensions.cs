using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Infrastructure;
using LZ4;
using MessagePack;

namespace DotnetSpider.Extensions
{
	public static class MessagePackSerializerExtensions
	{
		public static byte[] Serialize(this object message)
		{
			message.NotNull(nameof(message));
			var bytes = MessagePackSerializer.Typeless.Serialize(message);
			return LZ4Codec.Wrap(bytes);
		}

		public static async Task<object> DeserializeAsync(this byte[] bytes,
			CancellationToken cancellationToken = default)
		{
			bytes = LZ4Codec.Unwrap(bytes);
			var stream = new MemoryStream(bytes);
			return await MessagePackSerializer.Typeless.DeserializeAsync(stream, null, cancellationToken);
		}

		public static async Task<T> DeserializeAsync<T>(this byte[] bytes,
			CancellationToken cancellationToken = default)
			where T : class
		{
			var result = await bytes.DeserializeAsync(cancellationToken);
			return result as T;
		}
	}
}
