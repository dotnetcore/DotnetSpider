using System.IO;

namespace DotnetSpider.Common
{
	/// <summary>
	/// Stream 的扩展
	/// </summary>
	public static class StreamExtensions
	{
		/// <summary>
		/// Stream 转换成 byte[]
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <returns>byte[]</returns>
		public static byte[] ToBytes(this Stream input)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				input.CopyTo(ms);
				return ms.ToArray();
			}
		}
	}
}
