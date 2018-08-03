#if !NET40
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpider.Downloader
{
	public class GlobalRedirectHandler : DelegatingHandler
	{
		public GlobalRedirectHandler() : this(new HttpClientHandler
		{
			AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
			UseProxy = true,
			UseCookies = true,
			AllowAutoRedirect = false,
			MaxAutomaticRedirections = 10
		})
		{
		}

		public GlobalRedirectHandler(HttpMessageHandler innerHandler)
		{
			InnerHandler = innerHandler;
		}

		protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var response = await base.SendAsync(request, cancellationToken);

			if (response.StatusCode == HttpStatusCode.MovedPermanently
				|| response.StatusCode == HttpStatusCode.Moved
				|| response.StatusCode == HttpStatusCode.Redirect
				|| response.StatusCode == HttpStatusCode.Found
				|| response.StatusCode == HttpStatusCode.SeeOther
				|| response.StatusCode == HttpStatusCode.RedirectKeepVerb
				|| response.StatusCode == HttpStatusCode.TemporaryRedirect
				|| (int)response.StatusCode == 308)
			{

				var newRequest = CopyRequest(response.RequestMessage);

				if (response.StatusCode == HttpStatusCode.Redirect
					|| response.StatusCode == HttpStatusCode.Found
					|| response.StatusCode == HttpStatusCode.SeeOther)
				{
					newRequest.Content = null;
					newRequest.Method = HttpMethod.Get;
				}
				newRequest.RequestUri = new Uri(response.RequestMessage.RequestUri, response.Headers.Location);

				response = await SendAsync(newRequest, cancellationToken);

			}

			return response;
		}


		private static HttpRequestMessage CopyRequest(HttpRequestMessage oldRequest)
		{
			var newrequest = new HttpRequestMessage(oldRequest.Method, oldRequest.RequestUri);

			foreach (var header in oldRequest.Headers)
			{
				newrequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}
			foreach (var property in oldRequest.Properties)
			{
				newrequest.Properties.Add(property);
			}
			if (oldRequest.Content != null) newrequest.Content = new StreamContent(oldRequest.Content.ReadAsStreamAsync().Result);
			return newrequest;
		}
	}
}
#endif