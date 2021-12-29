using System.Security.Cryptography;
using Murmur;

namespace DotnetSpider.Infrastructure
{
	public class MurmurHashAlgorithmService : HashAlgorithmService
	{
		private readonly HashAlgorithm _hashAlgorithm;

		public MurmurHashAlgorithmService()
		{
			_hashAlgorithm = MurmurHash.Create32();
		}

		protected override HashAlgorithm GetHashAlgorithm()
		{
			return _hashAlgorithm;
		}
	}
}
