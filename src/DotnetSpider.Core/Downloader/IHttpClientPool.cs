using System;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// HttpClient信息封装
	/// </summary>
	public class HttpClientElement
	{
		/// <summary>
		/// <see cref="HttpClient"/>
		/// </summary>
		public HttpClient Client { get; set; }

		/// <summary>
		/// <see cref="HttpClientHandler"/>
		/// </summary>
		public HttpClientHandler Handler { get; set; }

		/// <summary>
		/// 上一次使用的时间
		/// </summary>
		public DateTime LastUsedTime { get; set; }
	}

	/// <summary>
	/// HttpClient池
	/// </summary>
	public interface IHttpClientPool
	{
		/// <summary>
		/// 通过不同的Hash分组, 返回对应的HttpClient
		/// 设计初衷: 某些网站会对COOKIE某部分做承上启下的检测, 因此必须保证: www.a.com/keyword=xxxx&amp;page=1 www.a.com/keyword=xxxx&amp;page=2 在同一个HttpClient里访问
		/// </summary>
		/// <param name="spider">爬虫</param>
		/// <param name="downloader">下载器</param>
		/// <param name="cookieContainer">Cookie</param>
		/// <param name="hashCode">分组的哈希</param>
		/// <param name="cookieInjector">Cookie注入器</param>
		/// <returns>HttpClientItem</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		HttpClientElement GetHttpClient(ISpider spider, IDownloader downloader, CookieContainer cookieContainer, int? hashCode = null, ICookieInjector cookieInjector = null);

		/// <summary>
		/// 更新池中所有HttpClient对象的 Cookie
		/// </summary>
		/// <param name="cookie">Cookie</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		void AddCookie(Cookie cookie);
	}
}
