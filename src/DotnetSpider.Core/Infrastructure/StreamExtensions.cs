using System.IO;

namespace DotnetSpider.Core.Infrastructure
{
	public static class StreamUtils
	{
		public static byte[] StreamToBytes(this Stream stream)
		{
			stream.Seek(0, SeekOrigin.Begin);
			byte[] bytes = new byte[stream.Length];
			stream.Read(bytes, 0, bytes.Length);
			// 设置当前流的位置为流的开始
			stream.Seek(0, SeekOrigin.Begin);
			return bytes;
		}
	}
}
