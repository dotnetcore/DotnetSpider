using System.Net;

namespace DotnetSpider.Core.Proxy
{
	public interface IHttpProxyPool
	{
		UseSpecifiedUriWebProxy GetProxy();
		void ReturnProxy(UseSpecifiedUriWebProxy host, HttpStatusCode statusCode);
	}
}