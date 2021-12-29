using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Infrastructure;

namespace DotnetSpider.Proxy
{
	public class ProxyHttpClientHandler : HttpClientHandler
	{
		internal IProxyService ProxyService { get; set; }

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			HttpResponseMessage response = null;
			var isProxyTestUrl = request.Headers.TryGetValues(Const.ProxyTestUrl, out _);
			if (isProxyTestUrl)
			{
				request.Headers.Remove(Const.ProxyTestUrl);
			}

			try
			{
				response = await base.SendAsync(request, cancellationToken);
				return response;
			}
			finally
			{
				if (!isProxyTestUrl)
				{
					var code = response?.StatusCode ?? HttpStatusCode.BadGateway;
					var webProxy = (WebProxy)Proxy;
					await ProxyService.ReturnAsync(webProxy.Address, code);
				}
			}
		}
	}
}
