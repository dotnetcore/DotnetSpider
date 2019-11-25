using System;
using System.Net;
using System.Text;
using DotnetSpider.Common;
using DotnetSpider.Selector;

namespace DotnetSpider.Downloader
{
	public static class ResponseExtensions
	{
		public static ISelectable ToSelectable(this Response response,
			ContentType type = ContentType.Auto, bool removeOutboundLinks = true)
		{
			var content = response.GetRawtext();
			switch (type)
			{
				case ContentType.Auto:
				{
					return IsJson(content)
						? new Selectable(content)
						: new Selectable(content, response.Request.Url, removeOutboundLinks);
				}

				case ContentType.Html:
				{
					content = WebUtility.HtmlDecode(content);
					return new Selectable(content, response.Request.Url, removeOutboundLinks);
				}

				case ContentType.Json:
				{
					if (IsJson(content))
					{
						return new Selectable(content);
					}

					throw new SpiderException("内容不是合法的 Json");
				}

				default:
				{
					throw new NotSupportedException();
				}
			}
		}

		public static string GetRawtext(this Response response)
		{
			return ReadContent(response.Request, response.Content, response.CharSet);
		}

		private static bool IsJson(string content)
		{
			return content.StartsWith("[") || content.StartsWith("{");
		}

		public static string ReadContent(Request request, byte[] contentBytes, string characterSet)
		{
			if (string.IsNullOrEmpty(request.Encoding))
			{
				var htmlCharset = EncodingHelper.GetEncoding(characterSet, contentBytes);
				return htmlCharset.GetString(contentBytes, 0, contentBytes.Length);
			}

			return Encoding.GetEncoding(request.Encoding).GetString(contentBytes, 0, contentBytes.Length);
		}
	}
}
