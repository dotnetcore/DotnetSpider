using System;
using System.Collections.Generic;

namespace DotnetSpider.Http
{
	public class ContentHeaders
		: Dictionary<string, dynamic>
	{
		public string Allow
		{
			get => GetHeader(HeaderNames.Allow);
			set => this[HeaderNames.Allow] = value;
		}

		public string ContentDisposition
		{
			get => GetHeader(HeaderNames.ContentDisposition);
			set => this[HeaderNames.ContentDisposition] = value;
		}

		public string ContentEncoding
		{
			get => GetHeader(HeaderNames.ContentEncoding);
			set => this[HeaderNames.ContentEncoding] = value;
		}

		public string ContentLanguage
		{
			get => GetHeader(HeaderNames.ContentLanguage);
			set => this[HeaderNames.ContentLanguage] = value;
		}

		public long? ContentLength
		{
			get => GetHeader(HeaderNames.ContentLength);
			set => this[HeaderNames.ContentLength] = value;
		}

		public string ContentLocation
		{
			get => GetHeader(HeaderNames.ContentLocation);
			set => this[HeaderNames.ContentLocation] = value;
		}

		// ReSharper disable once InconsistentNaming
		public string ContentMD5
		{
			get => GetHeader(HeaderNames.ContentMD5);
			set => this[HeaderNames.ContentMD5] = value;
		}

		public string ContentRange
		{
			get => GetHeader(HeaderNames.ContentRange);
			set => this[HeaderNames.ContentRange] = value;
		}

		public string ContentType
		{
			get => GetHeader(HeaderNames.ContentType);
			internal set => this[HeaderNames.ContentType] = value;
		}

		public DateTimeOffset? Expires
		{
			get => GetHeader(HeaderNames.Expires);
			set => this[HeaderNames.Expires] = value;
		}

		public DateTimeOffset? LastModified
		{
			get => GetHeader(HeaderNames.LastModified);
			set => this[HeaderNames.LastModified] = value;
		}

		private dynamic GetHeader(string name)
		{
			return ContainsKey(name) ? this[name] : null;
		}
	}
}
