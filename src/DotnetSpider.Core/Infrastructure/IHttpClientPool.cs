using System;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Core.Infrastructure
{
	public class HttpClientEntry
	{
		private bool _inited;

		public DateTime ActiveTime { get; set; }
		public HttpClient Client { get; private set; }

		internal HttpClientHandler Handler { get; private set; }

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
				AllowAutoRedirect = false,
				MaxAutomaticRedirections = 10
			};
			Client = allowAutoRedirect ? new HttpClient(new GlobalRedirectHandler(Handler)) : new HttpClient(Handler);
			ActiveTime = DateTime.Now;

			configAction();

			Handler.CookieContainer = cookieContainerFactory();

			_inited = true;
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
		/// Get a <see cref="HttpClientEntry"/> from <see cref="IHttpClientPool"/>.
		/// Return same <see cref="HttpClientEntry"/> instance when <paramref name="hash"/> is same.
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
