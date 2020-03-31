using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using LZ4;
using MessagePack;

namespace DotnetSpider.Extensions
{
    public static class RequestExtensions
    {
        public static byte[] ToBytes(this Request request)
        {
            request.NotNull(nameof(request));
            var bytes = MessagePackSerializer.Typeless.Serialize(request);
            return LZ4Codec.Wrap(bytes);
        }

        public static Request ToRequest(this byte[] bytes)
        {
            bytes = LZ4Codec.Unwrap(bytes);
            return (Request) MessagePackSerializer.Typeless.Deserialize(bytes);
        }
    }
}