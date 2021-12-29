using System;
using System.Text;

namespace DotnetSpider.Http
{
	public class StringContent : IHttpContent
	{
		private ContentHeaders _headers;
		private bool _disposed;

		public ContentHeaders Headers => _headers ??= new ContentHeaders();

		/// <summary>
		/// 内容
		/// </summary>
		public string Content { get; private set; }

		public string EncodingName { get; private set; }

		public string MediaType { get; private set; }

		private StringContent()
		{
		}

		public StringContent(string content)
			: this(content, Encoding.UTF8)
		{
		}

		public StringContent(string content, Encoding encoding, string mediaType = "text/plain")
		{
			EncodingName = encoding.BodyName.ToUpper();
			MediaType = mediaType;
			Content = content;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing || _disposed)
			{
				return;
			}

			_disposed = true;

			_headers.Clear();
			_headers = null;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public object Clone()
		{
			var content = new StringContent {Content = Content, EncodingName = EncodingName, MediaType = MediaType};

			if (_headers != null)
			{
				foreach (var header in _headers)
				{
					content.Headers.Add(header.Key, header.Value);
				}
			}

			return content;
		}
	}
}
