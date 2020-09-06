using System.Text;
using Murmur;

namespace DotnetSpider.Infrastructure
{
	public static class MurmurHashHelper
	{
		private static readonly Murmur128 MurmurHash3 = MurmurHash.Create128();

		public static string GetMurmurHash(this string str)
		{
			var bytes = MurmurHash3.ComputeHash(Encoding.UTF8.GetBytes(str));
			return bytes.GetMurmurHash();
		}

		public static string GetMurmurHash(this byte[] bytes)
		{
			var sb = new StringBuilder();
			foreach (var b in bytes)
			{
				sb.AppendFormat("{0:x2}", b);
			}

			return sb.ToString();
		}
	}
}
