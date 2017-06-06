using System.Net;

namespace DotnetSpider.Core.Proxy
{
	public class SingleProxyPool : IHttpProxyPool
	{
		private readonly UseSpecifiedUriWebProxy _proxy;

		public SingleProxyPool(string url)
		{
			_proxy = new UseSpecifiedUriWebProxy(new System.Uri(url));
		}

		public SingleProxyPool(UseSpecifiedUriWebProxy proxy)
		{
			_proxy = proxy;
		}

		public UseSpecifiedUriWebProxy GetProxy()
		{
			return _proxy;
		}

		public void ReturnProxy(UseSpecifiedUriWebProxy host, HttpStatusCode statusCode)
		{
		}
	}
}