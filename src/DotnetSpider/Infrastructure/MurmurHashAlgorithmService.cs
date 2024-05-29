using System.Security.Cryptography;
using Murmur;

namespace DotnetSpider.Infrastructure;

public class MurmurHashAlgorithmService : HashAlgorithmService
{
    private readonly HashAlgorithm _hashAlgorithm = MurmurHash.Create128();

    protected override HashAlgorithm GetHashAlgorithm()
    {
        return _hashAlgorithm;
    }
}
