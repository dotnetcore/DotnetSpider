using System.Security.Cryptography;

namespace DotnetSpider.Infrastructure;

public class MD5HashAlgorithmService : HashAlgorithmService
{
    private readonly HashAlgorithm _hashAlgorithm = MD5.Create();

    protected override HashAlgorithm GetHashAlgorithm()
    {
        return _hashAlgorithm;
    }
}
