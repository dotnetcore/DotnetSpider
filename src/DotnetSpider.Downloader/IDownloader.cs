using DotnetSpider.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DotnetSpider.Downloader.Test")]

namespace DotnetSpider.Downloader
{
	public interface IDownloader : IDisposable
	{
		ILogger Logger { get; set; }

		/// <summary>
		/// Download content from a web url
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 下载链接内容
		/// </summary>
		/// <param name="request">链接请求 <see cref="Request"/></param>
		/// <returns>链接请求结果 <see cref="Response"/></returns>
		Response Download(Request request);

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

		///// <summary>
		///// Cookie管理容器
		///// </summary>
		//CookieContainer CookieContainer { get; set; }

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
		/// Clone a Downloader throuth <see cref="object.MemberwiseClone"/>, override if you need a deep clone or others. 
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 克隆一个下载器, 多线程时, 每个线程使用一个下载器对象, 这样如WebDriver下载器则不再需要管理WebDriver对象的个数了, 每个下载器就只包含一个WebDriver
		/// </summary>
		/// <returns>下载器</returns>
		IDownloader Clone();

		/// <summary>
		/// Cookie Injector.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// Cookie 注入器
		/// </summary>
		ICookieInjector CookieInjector { get; set; }
	}
}
