using System;
using System.Text;
using System.Text.RegularExpressions;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 编码扩展
	/// </summary>
	public static class EncodingExtensions
	{
		private const int Utf8PreambleLength = 3;
		private const byte Utf8PreambleByte2 = 0xBF;
		private const int Utf8PreambleFirst2Bytes = 0xEFBB;

		// UTF32 not supported on Phone
		private const int Utf32PreambleLength = 4;
		private const byte Utf32PreambleByte2 = 0x00;
		private const byte Utf32PreambleByte3 = 0x00;
		private const int Utf32OrUnicodePreambleFirst2Bytes = 0xFFFE;
		private const int BigEndianUnicodePreambleFirst2Bytes = 0xFEFF;

		/// <summary>
		/// 检测编码类型
		/// </summary>
		/// <param name="characterSet">编码名称</param>
		/// <param name="bytes">被检测的编码</param>
		/// <returns>编码类型</returns>
		public static Encoding GetEncoding(string characterSet, byte[] bytes)
		{
			if (!string.IsNullOrEmpty(characterSet))
			{
				return GetEncoding(characterSet);
			}
			else
			{
				Match meta = Regex.Match(Encoding.UTF8.GetString(bytes), "<meta[^<]*charset=([^<]*)[\"']", RegexOptions.IgnoreCase);
				string c = string.Empty;
				if (meta.Groups.Count > 0)
				{
					c = meta.Groups[1].Value.ToLower().Trim();
				}
				if (c.Length > 2)
				{
					try
					{
						return Encoding.GetEncoding(c.Replace("\"", string.Empty).Replace("'", "").Replace(";", "").Replace("iso-8859-1", "gbk").Trim());
					}
					catch
					{
						var buffer = new ArraySegment<byte>(bytes);
						return DetectEncoding(buffer);
					}
				}
				else
				{
					var buffer = new ArraySegment<byte>(bytes);
					return DetectEncoding(buffer);
				}
			}
		}

		private static Encoding GetEncoding(string characterSet)
		{
			var encodingName = characterSet.ToLower();
			if (encodingName.Contains("gb2312"))
			{
				return Encoding.GetEncoding("GB2312");
			}
			else if (encodingName.Contains("gbk"))
			{
				return Encoding.GetEncoding("GBK");
			}
			else if (encodingName.Contains("utf-8") || encodingName.Contains("utf8"))
			{
				return Encoding.UTF8;
			}
			else
			{
				try
				{
					return Encoding.GetEncoding(characterSet);
				}
				catch
				{
					return Encoding.UTF8;
				}
			}
		}

		private static Encoding DetectEncoding(ArraySegment<byte> buffer)
		{
			byte[] data = buffer.Array;
			int offset = buffer.Offset;
			int dataLength = buffer.Count;


			if (dataLength >= 2 && data != null)
			{
				int first2Bytes = data[offset + 0] << 8 | data[offset + 1];

				switch (first2Bytes)
				{
					case Utf8PreambleFirst2Bytes:
						if (dataLength >= Utf8PreambleLength && data[offset + 2] == Utf8PreambleByte2)
						{
							return Encoding.UTF8;
						}
						break;

					case Utf32OrUnicodePreambleFirst2Bytes:
#if !NETNative
						// UTF32 not supported on Phone
						if (dataLength >= Utf32PreambleLength && data[offset + 2] == Utf32PreambleByte2 && data[offset + 3] == Utf32PreambleByte3)
						{
							return Encoding.UTF32;
						}
						else
#endif
						{
							return Encoding.Unicode;
						}

					case BigEndianUnicodePreambleFirst2Bytes:
						return Encoding.BigEndianUnicode;

				}
			}

			return Encoding.UTF8;
		}
	}
}
