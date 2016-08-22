using System;
using System.Net;

namespace DotnetSpider.Core.Proxy
{
	public sealed class UseSpecifiedUriWebProxy : IWebProxy
	{
		public Uri Uri;
		private readonly bool _bypass;

		public UseSpecifiedUriWebProxy(Uri uri, ICredentials credentials = null, bool bypass = false)
		{
			Uri = uri;
			_bypass = bypass;
			Credentials = credentials;
		}

		public ICredentials Credentials { get; set; }
		public Uri GetProxy(Uri destination) => Uri;
		public bool IsBypassed(Uri host) => _bypass;
	}
}
