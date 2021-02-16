using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Infrastructure;
using MessagePack;

namespace DotnetSpider.Extensions
{
	public static class MessagePackSerializerExtensions
	{
		private static readonly MessagePackSerializerOptions _serializerOptions =
			MessagePackSerializer.Typeless.DefaultOptions.WithCompression(MessagePackCompression.Lz4Block);

		public static byte[] Serialize(this object message)
		{
			message.NotNull(nameof(message));

			var bytes = MessagePackSerializer.Typeless.Serialize(message, _serializerOptions);
			return bytes;
		}

		public static async Task<object> DeserializeAsync(this byte[] bytes,
			CancellationToken cancellationToken = default)
		{
			var stream = new MemoryStream(bytes);
			return await MessagePackSerializer.Typeless.DeserializeAsync(stream, _serializerOptions, cancellationToken);
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
