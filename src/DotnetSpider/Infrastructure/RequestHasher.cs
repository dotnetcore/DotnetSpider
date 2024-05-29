using System;
using DotnetSpider.Extensions;
using DotnetSpider.Http;

namespace DotnetSpider.Infrastructure;

/// <summary>
///请求哈希编译器
/// </summary>
public class RequestHasher(IHashAlgorithmService hashAlgorithmService) : IRequestHasher
{
    public void ComputeHash(Request request)
    {
        var bytes = new
        {
            request.Owner,
            request.RequestUri.AbsoluteUri,
            request.Method,
            request.RequestedTimes,
            request.Content
        }.Serialize();
        var hash64 = BitConverter.ToUInt64(hashAlgorithmService.ComputeHash(bytes), 0);
        request.Hash = $"{hash64:X}";
    }
}
