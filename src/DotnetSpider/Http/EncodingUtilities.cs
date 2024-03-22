#nullable enable
using System.Text;
using System.Text.RegularExpressions;

namespace DotnetSpider.Http
{
	internal static class EncodingUtilities
	{
		private static readonly Regex _metaTagRegex = new ("""<meta.*?charset="?(?<charset>[a-zA-Z0-9-]+).*?>""", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public static Encoding? GetEncodingFromMetaTag(this byte[] bytes)
		{
			var html = Encoding.ASCII.GetString(bytes);

			var match = _metaTagRegex.Match(html);

			if (match.Success)
			{
				var charset = match.Groups["charset"].Value;

				if (string.IsNullOrWhiteSpace(charset))
				{
					return null;
				}

				try
				{
					return Encoding.GetEncoding(charset);
				}
				catch
				{
					return null;
				}
			}

			return null;
		}

		public static Encoding? GetEncodingFromBom(this byte[] bytes)
		{
			if (bytes.Length < 2)
			{
				return null;
			}

			if (bytes[0] == 0xFE && bytes[1] == 0xFF)
			{
				return Encoding.BigEndianUnicode;
			}

			if (bytes[0] == 0xFF && bytes[1] == 0xFE)
			{
				return Encoding.Unicode;
			}

			if (bytes.Length < 3)
			{
				return null;
			}

			if (bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
			{
				return Encoding.UTF8;
			}

			return null;
		}

		private static readonly Regex _charsetPattern = new("charset=(?<charset>[a-zA-Z0-9-]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public static Encoding? GetEncodingFromContentType(this ContentHeaders contentHeaders)
		{
			var contentType = contentHeaders.ContentType;

			if (string.IsNullOrWhiteSpace(contentType))
			{
				return null;
			}

			var match = _charsetPattern.Match(contentType);

			if (match.Success)
			{
				var charset = match.Groups["charset"].Value;

				if (string.IsNullOrWhiteSpace(charset))
				{
					return null;
				}

				try
				{
					return Encoding.GetEncoding(charset);
				}
				catch
				{
					return null;
				}
			}

			return null;
		}

		public static Encoding GetEncoding(this ByteArrayContent content)
		{
			var encoding = content.Headers.GetEncodingFromContentType();

			if (encoding != null)
			{
				return encoding;
			}

			encoding = content.Bytes.GetEncodingFromMetaTag();

			if (encoding != null)
			{
				return encoding;
			}

			encoding = content.Bytes.GetEncodingFromBom();

			return encoding ?? Encoding.UTF8;
		}
	}
}
