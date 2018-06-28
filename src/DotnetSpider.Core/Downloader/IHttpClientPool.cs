using System;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Downloader
{
	public class HttpClientEntry
	{
		private bool _inited;

		public DateTime ActiveTime { get; set; }
		public HttpClient Client { get; private set; }

		internal HttpClientHandler Handler { get; private set; }

		internal CookieContainer CookieContainer
		{
			set
			{
				if (_inited)
				{
					return;
				}
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void Init(bool allowAutoRedirect, Action configAction, Func<CookieContainer> cookieContainerFactory)
		{
			if (_inited)
			{
				return;
			}

			Handler = new HttpClientHandler
			{
				AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
				UseProxy = true,
				UseCookies = true,
				AllowAutoRedirect = allowAutoRedirect,
				MaxAutomaticRedirections = 10
			};
			Client = allowAutoRedirect ? new HttpClient(new GlobalRedirectHandler(Handler)) : new HttpClient(Handler);
			ActiveTime = DateTime.Now;

			configAction();

			Handler.CookieContainer = cookieContainerFactory();

			_inited = true;
		}

		public class GlobalRedirectHandler : DelegatingHandler
		{
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

	/// <summary>
	/// Httpclient pool impletion for <see cref="IHttpClientPool"/>
	/// 一旦Handler与目标地址建立了连接, Handler中的CookieContainer、Proxy等设置都是不能再更改的
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// HttpClient池
	/// </summary>
	public interface IHttpClientPool
	{
		/// <summary>
		/// Get a <see cref="HttpClientElement"/> from <see cref="IHttpClientPool"/>.
		/// Return same <see cref="HttpClientElement"/> instance when <paramref name="hashCode"/> is same.
		/// This can ensure some pages have same CookieContainer.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 通过不同的Hash分组, 返回对应的HttpClient
		/// 设计初衷: 某些网站会对COOKIE某部分做承上启下的检测, 因此必须保证: www.a.com/keyword=xxxx&amp;page=1 www.a.com/keyword=xxxx&amp;page=2 在同一个HttpClient里访问
		/// </summary>
		/// <param name="hash">分组的哈希 Hashcode to identify different group.</param>
		/// <returns>HttpClientItem</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		HttpClientEntry GetHttpClient(string hash);

		/// <summary>
		/// Add cookie to <see cref="IHttpClientPool"/>
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 更新池中所有HttpClient对象的 Cookie
		/// </summary>
		/// <param name="cookie">Cookie</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		void AddCookie(Cookie cookie);
	}
}
