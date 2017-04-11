using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Core.Proxy;

namespace DotnetSpider.Core.Downloader
{
	public class HttpClientPool
	{
		public class HttpClientObj
		{
			public HttpClient Client { get; set; }
			public DateTime Start { get; set; }
		}

		private readonly Dictionary<int, HttpClientObj> _pool = new Dictionary<int, HttpClientObj>();
		private readonly HttpClient _noProxyHttpClient = new HttpClient(new GlobalRedirectHandler(new HttpClientHandler
		{
			AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
			UseProxy = true,
			UseCookies = false
		}));

		public HttpClient GetHttpClient(UseSpecifiedUriWebProxy proxy)
		{
			if (proxy == null)
			{
				return _noProxyHttpClient;
			}

			var key = proxy.GetHashCode();
			if (key == -1)
			{
				return _noProxyHttpClient;
			}

			if (_pool.Count % 100 == 0)
			{
				ClearHttpClient();
			}

			if (_pool.ContainsKey(key))
			{
				return _pool[key].Client;
			}
			else
			{
				var client = new HttpClient(new GlobalRedirectHandler(new HttpClientHandler
				{
					AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
					UseProxy = true,
					UseCookies = false,
					Proxy = proxy
				}));
				_pool.Add(key, new HttpClientObj
				{
					Client = client,
					Start = DateTime.Now
				});
				return client;
			}
		}

		private void ClearHttpClient()
		{
			List<int> needRemoveList = new List<int>();
			var now = DateTime.Now;
			foreach (var pair in _pool)
			{
				if ((now - pair.Value.Start).TotalSeconds > 240)
				{
					needRemoveList.Add(pair.Key);
				}
			}

			foreach (var key in needRemoveList)
			{
				_pool.Remove(key);
			}
		}

		public class GlobalRedirectHandler : DelegatingHandler
		{
			public GlobalRedirectHandler(HttpMessageHandler innerHandler)
			{
				InnerHandler = innerHandler;
			}

			private Task<HttpResponseMessage> _SendAsync(HttpRequestMessage request, CancellationToken cancellationToken, TaskCompletionSource<HttpResponseMessage> tcs)
			{
				base.SendAsync(request, cancellationToken)
					.ContinueWith(t =>
					{
						HttpResponseMessage response;
						try
						{
							response = t.Result;
						}
						catch (Exception e)
						{
							response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { ReasonPhrase = e.Message };
						}
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

							newRequest.RequestUri = response.Headers.Location;

							_SendAsync(newRequest, cancellationToken, tcs);
						}
						else
						{
							tcs.SetResult(response);
						}
					}, cancellationToken);

				return tcs.Task;
			}

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				var tcs = new TaskCompletionSource<HttpResponseMessage>();
				return _SendAsync(request, cancellationToken, tcs);
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
}
