using System;
using System.Net;

namespace DotnetSpider.Core.Proxy
{
	public interface IHttpProxyPool : IDisposable
	{
		UseSpecifiedUriWebProxy GetProxy();
		void ReturnProxy(UseSpecifiedUriWebProxy host, HttpStatusCode statusCode);
	}
}