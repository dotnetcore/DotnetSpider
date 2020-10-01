using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpider.Proxy
{
	public class ProxyHttpClientHandler : HttpClientHandler
	{
		public IProxyService ProxyService { get; set; }

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			HttpResponseMessage response = null;
			try
			{
				response = await base.SendAsync(request, cancellationToken);
				return response;
			}
			finally
			{
				var code = response?.StatusCode ?? HttpStatusCode.BadGateway;
				var webProxy = (WebProxy)Proxy;
				await ProxyService.ReturnAsync(webProxy.Address, code);
			}
		}
	}
}
