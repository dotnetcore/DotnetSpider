using System.IO;

namespace DotnetSpider.Core.Infrastructure
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
		public static byte[] ToBytes(this Stream stream)
		{
			byte[] bytes = new byte[stream.Length];
			stream.Read(bytes, 0, bytes.Length);
			// 设置当前流的位置为流的开始
			stream.Seek(0, SeekOrigin.Begin);
			return bytes;
		}
	}
}
