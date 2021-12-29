using System;
using System.Collections.Generic;

namespace DotnetSpider.Http
{
	public class RequestHeaders : Dictionary<string, dynamic>
	{
		public string Accept
		{
			get => GetHeader(HeaderNames.Accept);
			set => this[HeaderNames.Accept] = value;
		}

		public string AcceptCharset
		{
			get => GetHeader(HeaderNames.AcceptCharset);
			set => this[HeaderNames.AcceptCharset] = value;
		}

		public string AcceptEncoding
		{
			get => GetHeader(HeaderNames.AcceptEncoding);
			set => this[HeaderNames.AcceptEncoding] = value;
		}

		public string AcceptLanguage
		{
			get => GetHeader(HeaderNames.AcceptLanguage);
			set => this[HeaderNames.AcceptLanguage] = value;
		}

		public string Authorization
		{
			get => GetHeader(HeaderNames.Authorization);
			set => this[HeaderNames.Authorization] = value;
		}

		public string Expect
		{
			get => GetHeader(HeaderNames.Expect);
			set => this[HeaderNames.Expect] = value;
		}

		public string From
		{
			get => GetHeader(HeaderNames.From);
			set => this[HeaderNames.From] = value;
		}

		public string Host
		{
			get => GetHeader(HeaderNames.Host);
			set => this[HeaderNames.Host] = value;
		}

		public string IfMatch
		{
			get => GetHeader(HeaderNames.IfMatch);
			set => this[HeaderNames.IfMatch] = value;
		}

		public DateTimeOffset? IfModifiedSince { get; set; }

		public string IfNoneMatch
		{
			get => GetHeader(HeaderNames.IfNoneMatch);
			set => this[HeaderNames.IfNoneMatch] = value;
		}

		public string ProxyAuthorization
		{
			get => GetHeader(HeaderNames.ProxyAuthorization);
			set => this[HeaderNames.ProxyAuthorization] = value;
		}

		public string Range
		{
			get => GetHeader(HeaderNames.Range);
			set => this[HeaderNames.Range] = value;
		}

		public string Referrer
		{
			get => GetHeader(HeaderNames.Referer);
			set => this[HeaderNames.Referer] = value;
		}

		public string UserAgent
		{
			get => GetHeader(HeaderNames.UserAgent);
			set => this[HeaderNames.UserAgent] = value;
		}

		// ReSharper disable once InconsistentNaming
		public string TE
		{
			get => GetHeader(HeaderNames.TE);
			set => this[HeaderNames.TE] = value;
		}

		public string Connection
		{
			get => GetHeader(HeaderNames.Connection);
			set => this[HeaderNames.Connection] = value;
		}

		public string Pragma
		{
			get => GetHeader(HeaderNames.Pragma);
			set => this[HeaderNames.Pragma] = value;
		}

		public string Trailer
		{
			get => GetHeader(HeaderNames.Trailer);
			set => this[HeaderNames.Trailer] = value;
		}

		public string TransferEncoding
		{
			get => GetHeader(HeaderNames.TransferEncoding);
			set => this[HeaderNames.TransferEncoding] = value;
		}

		public string Upgrade
		{
			get => GetHeader(HeaderNames.Upgrade);
			set => this[HeaderNames.Upgrade] = value;
		}

		public string Via
		{
			get => GetHeader(HeaderNames.Via);
			set => this[HeaderNames.Via] = value;
		}

		public string Warning
		{
			get => GetHeader(HeaderNames.Warning);
			set => this[HeaderNames.Warning] = value;
		}

		public string Cookie
		{
			get => GetHeader(HeaderNames.Cookie);
			set => this[HeaderNames.Cookie] = value;
		}

		public DateTimeOffset? Date
		{
			get => GetHeader(HeaderNames.Date);
			set => this[HeaderNames.Date] = value;
		}

		public DateTimeOffset? IfUnmodifiedSince
		{
			get => GetHeader(HeaderNames.IfUnmodifiedSince);
			set => this[HeaderNames.IfUnmodifiedSince] = value;
		}

		public int? MaxForwards
		{
			get => GetHeader(HeaderNames.MaxForwards);
			set => this[HeaderNames.MaxForwards] = value;
		}

		public bool? ExpectContinue => Expect?.ToLower().Contains("continue");

		public bool? ConnectionClose => Connection?.ToLower().Contains("close");

		private dynamic GetHeader(string name)
		{
			return ContainsKey(name) ? this[name] : null;
		}
	}
}
