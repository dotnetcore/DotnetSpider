using System;
using System.Collections.Generic;
using System.Net;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// 下载器接口
	/// </summary>
	public interface IDownloader : IDisposable
	{
		/// <summary>
		/// 下载链接内容
		/// </summary>
		/// <param name="request">链接请求</param>
		/// <param name="spider">爬虫接口</param>
		/// <returns>下载内容封装好的页面对象</returns>
		Page Download(Request request, ISpider spider);

		/// <summary>
		/// 添加下载完成后的后续处理操作
		/// </summary>
		/// <param name="handler"><see cref="IAfterDownloadCompleteHandler"/></param>
		void AddAfterDownloadCompleteHandler(IAfterDownloadCompleteHandler handler);

		/// <summary>
		/// 添加下载操作前的处理操作
		/// </summary>
		/// <param name="handler"><see cref="IBeforeDownloadHandler"/></param>
		void AddBeforeDownloadHandler(IBeforeDownloadHandler handler);

		/// <summary>
		/// 设置 Cookie
		/// </summary>
		/// <param name="cookie">Cookie</param>
		void AddCookie(Cookie cookie);

		/// <summary>
		/// 设置 Cookie
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="value">Value</param>
		/// <param name="domain">作用域</param>
		/// <param name="path">作用路径</param>
		void AddCookie(string name, string value, string domain, string path = "/");

		/// <summary>
		/// 添加Cookies
		/// </summary>
		/// <param name="cookies">Cookies的键值对</param>
		/// <param name="domain">作用域</param>
		/// <param name="path">作用路径</param>
		void AddCookies(IDictionary<string, string> cookies, string domain, string path = "/");

		/// <summary>
		/// 设置 Cookies
		/// </summary>
		/// <param name="cookiesStr">Cookies的键值对字符串, 如: a1=b;a2=c;</param>
		/// <param name="domain">作用域</param>
		/// <param name="path">作用路径</param>
		void AddCookies(string cookiesStr, string domain, string path = "/");

		/// <summary>
		/// Cookie 注入器
		/// </summary>
		ICookieInjector CookieInjector { get; set; }

		/// <summary>
		/// 克隆一个下载器, 多线程时, 每个线程使用一个下载器对象, 这样如WebDriver下载器则不再需要管理WebDriver对象的个数了, 每个下载器就只包含一个WebDriver
		/// </summary>
		/// <returns>下载器</returns>
		IDownloader Clone();

		/// <summary>
		/// Gets a System.Net.CookieCollection that contains the System.Net.Cookie instances that are associated with a specific URI.
		/// </summary>
		/// <param name="uri">The URI of the System.Net.Cookie instances desired.</param>
		/// <returns>A System.Net.CookieCollection that contains the System.Net.Cookie instances that are associated with a specific URI.</returns>
		CookieCollection GetCookies(Uri uri);
	}
}
