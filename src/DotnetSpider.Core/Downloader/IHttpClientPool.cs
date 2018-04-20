using System;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// HttpClient Infomations
	/// </summary>
	/// <summary xml:lang="zh-CN">
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
		/// The last time this is used.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 上一次使用的时间
		/// </summary>
		public DateTime LastUsedTime { get; set; }

		public override int GetHashCode()
		{
			return (Client.GetHashCode() + Handler.Proxy.ToString()).GetHashCode();
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
		/// <param name="spider">爬虫 <see cref="ISpider"/></param>
		/// <param name="downloader">下载器 <see cref="IDownloader"/></param>
		/// <param name="cookieContainer">Cookie <see cref="CookieContainer"/></param>
		/// <param name="hash">分组的哈希 Hashcode to identify different group.</param>
		/// <param name="cookieInjector">Cookie注入器 <see cref="ICookieInjector"/></param>
		/// <returns>HttpClientItem</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		HttpClientElement GetHttpClient(ISpider spider, IDownloader downloader, CookieContainer cookieContainer, string hash, ICookieInjector cookieInjector = null);

		[MethodImpl(MethodImplOptions.Synchronized)]
		HttpClientElement GetHttpClient(ISpider spider, IDownloader downloader, CookieContainer cookieContainer, IWebProxy proxy, ICookieInjector cookieInjector = null);

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
