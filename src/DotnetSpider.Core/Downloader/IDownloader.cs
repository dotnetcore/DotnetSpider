using System;
using System.Collections.Generic;
using System.Net;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// Downloader interface
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// 下载器接口
	/// </summary>
	public interface IDownloader : IDisposable
	{
		/// <summary>
		/// Download content from a web url
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 下载链接内容
		/// </summary>
		/// <param name="request">链接请求 <see cref="Request"/></param>
		/// <param name="spider">爬虫接口 <see cref="ISpider"/></param>
		/// <returns>下载内容封装好的页面对象 <see cref="Page"/></returns>
		Page Download(Request request, ISpider spider);

		/// <summary>
		/// Add handlers for post-processing.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 添加下载完成后的后续处理操作
		/// </summary>
		/// <param name="handler"><see cref="IAfterDownloadCompleteHandler"/></param>
		void AddAfterDownloadCompleteHandler(IAfterDownloadCompleteHandler handler);

		/// <summary>
		/// Add handlers for pre-processing.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 添加下载操作前的处理操作
		/// </summary>
		/// <param name="handler"><see cref="IBeforeDownloadHandler"/></param>
		void AddBeforeDownloadHandler(IBeforeDownloadHandler handler);

		/// <summary>
		/// Add cookies.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 设置 Cookie
		/// </summary>
		/// <param name="cookie">Cookie <see cref="Cookie"/></param>
		void AddCookie(Cookie cookie);

		/// <summary>
		/// Add cookies.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 设置 Cookie
		/// </summary>
		/// <param name="name">名称(<see cref="Cookie.Name"/>)</param>
		/// <param name="value">值(<see cref="Cookie.Value"/>)</param>
		/// <param name="domain">作用域(<see cref="Cookie.Domain"/>)</param>
		/// <param name="path">作用路径(<see cref="Cookie.Path"/>)</param>
		void AddCookie(string name, string value, string domain, string path = "/");

		/// <summary>
		/// Add cookies to downloader
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 添加Cookies
		/// </summary>
		/// <param name="cookies">Cookies的键值对 (Cookie's key-value pairs)</param>
		/// <param name="domain">作用域(<see cref="Cookie.Domain"/>)</param>
		/// <param name="path">作用路径(<see cref="Cookie.Path"/>)</param>
		void AddCookies(IDictionary<string, string> cookies, string domain, string path = "/");

		/// <summary>
		/// Add cookies to downloader
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 设置 Cookies
		/// </summary>
		/// <param name="cookiesStr">Cookies的键值对字符串, 如: a1=b;a2=c;(Cookie's key-value pairs string, a1=b;a2=c; etc.)</param>
		/// <param name="domain">作用域(<see cref="Cookie.Domain"/>)</param>
		/// <param name="path">作用路径(<see cref="Cookie.Path"/>)</param>
		void AddCookies(string cookiesStr, string domain, string path = "/");

		/// <summary>
		/// Cookie Injector.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// Cookie 注入器
		/// </summary>
		ICookieInjector CookieInjector { get; set; }

		/// <summary>
		/// Clone a Downloader throuth <see cref="object.MemberwiseClone"/>, override if you need a deep clone or others. 
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 克隆一个下载器, 多线程时, 每个线程使用一个下载器对象, 这样如WebDriver下载器则不再需要管理WebDriver对象的个数了, 每个下载器就只包含一个WebDriver
		/// </summary>
		/// <returns>下载器</returns>
		IDownloader Clone();

		/// <summary>
		/// Gets a <see cref="System.Net.CookieCollection"/> that contains the <see cref="System.Net.Cookie"/> instances that are associated with a specific <see cref="Uri"/>.
		/// </summary>
		/// <param name="uri">The URI of the System.Net.Cookie instances desired.</param>
		/// <returns>A System.Net.CookieCollection that contains the System.Net.Cookie instances that are associated with a specific URI.</returns>
		CookieCollection GetCookies(Uri uri);
	}
}
