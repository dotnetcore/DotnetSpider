using DotnetSpider.Core.Infrastructure;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// The Abstraction of a basic downloader.
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// 基础下载器的抽象
	/// </summary>
	public abstract class BaseDownloader : Named, IDownloader
	{
		/// <summary>
		/// Log interface.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 日志接口
		/// </summary>
		protected static readonly ILogger Logger = DLog.GetLogger();

		/// <summary>
		/// Whether the downloader should automaticlly detect <see cref="Site.ContentType"/>.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 是否检测过下载内容的类型
		/// </summary>
		private bool _detectedContentType;
		private readonly List<IAfterDownloadCompleteHandler> _afterDownloadCompletes = new List<IAfterDownloadCompleteHandler>();
		private readonly List<IBeforeDownloadHandler> _beforeDownloads = new List<IBeforeDownloadHandler>();

		/// <summary>
		/// Cookie Container
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// Cookie 容器
		/// </summary>
		protected readonly CookieContainer CookieContainer = new CookieContainer();

		/// <summary>
		/// Interface to inject cookie.
		/// </summary>
		public ICookieInjector CookieInjector { get; set; }

		/// <summary>
		/// Add cookies to downloader
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 设置 Cookies
		/// </summary>
		/// <param name="cookiesStr">Cookies的键值对字符串, 如: a1=b;a2=c;(Cookie's key-value pairs string, a1=b;a2=c; etc.)</param>
		/// <param name="domain">作用域(<see cref="Cookie.Domain"/>)</param>
		/// <param name="path">作用路径(<see cref="Cookie.Path"/>)</param>
		public void AddCookies(string cookiesStr, string domain, string path = "/")
		{
			var cookies = new Dictionary<string, string>();
			var pairs = cookiesStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var pair in pairs)
			{
				var keyValue = pair.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
				var name = keyValue[0];
				string value = keyValue.Length > 1 ? keyValue[1] : string.Empty;
				AddCookie(name, value, domain, path);
			}
		}

		/// <summary>
		/// Add cookies to downloader
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 添加Cookies
		/// </summary>
		/// <param name="cookies">Cookies的键值对 (Cookie's key-value pairs)</param>
		/// <param name="domain">作用域(<see cref="Cookie.Domain"/>)</param>
		/// <param name="path">作用路径(<see cref="Cookie.Path"/>)</param>
		public void AddCookies(IDictionary<string, string> cookies, string domain, string path = "/")
		{
			foreach (var pair in cookies)
			{
				var name = pair.Key;
				var value = pair.Value;
				AddCookie(name, value, domain, path);
			}
		}

		/// <summary>
		/// Add one cookie to downloader
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 添加Cookie
		/// </summary>
		/// <param name="name">名称(<see cref="Cookie.Name"/>)</param>
		/// <param name="value">值(<see cref="Cookie.Value"/>)</param>
		/// <param name="domain">作用域(<see cref="Cookie.Domain"/>)</param>
		/// <param name="path">作用路径(<see cref="Cookie.Path"/>)</param>
		public void AddCookie(string name, string value, string domain, string path = "/")
		{
			var cookie = new Cookie(name.Trim(), value.Trim(), path.Trim(), domain.Trim());
			AddCookie(cookie);
		}

		/// <summary>
		/// Gets a <see cref="System.Net.CookieCollection"/> that contains the <see cref="System.Net.Cookie"/> instances that are associated with a specific <see cref="Uri"/>.
		/// </summary>
		/// <param name="uri">The URI of the System.Net.Cookie instances desired.</param>
		/// <returns>A <see cref="System.Net.CookieCollection"/> that contains the <see cref="System.Net.Cookie"/> instances that are associated with a specific <see cref="Uri"/>.</returns>
		public CookieCollection GetCookies(Uri uri)
		{
			return CookieContainer.GetCookies(uri);
		}

		/// <summary>
		/// Add one cookie to downloader
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 设置 Cookie
		/// </summary>
		/// <param name="cookie">Cookie</param>
		public void AddCookie(Cookie cookie)
		{
			if (cookie == null)
			{
				return;
			}
			CookieContainer.Add(cookie);
			AddCookieToDownloadClient(cookie);
		}

		/// <summary>
		/// Override this method to add cookies to Downloaders (HttpClient, WebDriver etc.) 
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 下载器在第一次使用时, 会把CookieContainer中的Cookie加载到下载工具中(HttpClient, WebDriver), 
		/// 但当下载器已经在运行时, 更新Cookie则需要使用此方法把新的Cookie更新到各个下载工具中
		/// </summary>
		/// <param name="cookie">Cookie</param>
		protected virtual void AddCookieToDownloadClient(Cookie cookie) { }

		/// <summary>
		/// Download webpage content and build a <see cref="Page"/> instance.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 下载链接内容
		/// </summary>
		/// <param name="request">链接请求 <see cref="Request"/></param>
		/// <param name="spider">爬虫接口 <see cref="ISpider"/></param>
		/// <returns>下载内容封装好的页面对象 (a <see cref="Page"/> instance that contains requested page infomations, like Html source, headers, etc.)</returns>
		public Page Download(Request request, ISpider spider)
		{
			BeforeDownload(ref request, spider);
			var page = DowloadContent(request, spider);
			AfterDownloadComplete(ref page, spider);
			TryDetectContentType(page, spider);
			return page;
		}

		/// <summary>
		/// Add a <see cref="IAfterDownloadCompleteHandler"/> to <see cref="IDownloader"/>
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 添加处理器
		/// </summary>
		/// <param name="handler"><see cref="IAfterDownloadCompleteHandler"/></param>
		public void AddAfterDownloadCompleteHandler(IAfterDownloadCompleteHandler handler)
		{
			_afterDownloadCompletes.Add(handler);
		}

		/// <summary>
		/// Add a <see cref="IBeforeDownloadHandler"/> to <see cref="IDownloader"/>
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 添加处理器
		/// </summary>
		/// <param name="handler"><see cref="IBeforeDownloadHandler"/></param>
		public void AddBeforeDownloadHandler(IBeforeDownloadHandler handler)
		{
			_beforeDownloads.Add(handler);
		}

		/// <summary>
		/// Clone a Downloader throuth <see cref="object.MemberwiseClone"/>, override if you need a deep clone or others. 
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 克隆一个下载器, 多线程时, 每个线程使用一个下载器, 这样如WebDriver下载器则不再需要管理WebDriver对象的个数了, 每个下载器就只包含一个WebDriver。
		/// </summary>
		/// <returns>下载器</returns>
		public virtual IDownloader Clone()
		{
			return MemberwiseClone() as IDownloader;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose()
		{
		}

		/// <summary>
		/// Override this method to download content.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 下载工作的具体实现
		/// </summary>
		/// <param name="request">请求信息 <see cref="Request"/></param>
		/// <param name="spider">爬虫 <see cref="ISpider"/></param>
		/// <returns>页面数据 <see cref="Page"/></returns>
		protected abstract Page DowloadContent(Request request, ISpider spider);

		private void BeforeDownload(ref Request request, ISpider spider)
		{
			if (_beforeDownloads != null && _beforeDownloads.Count > 0)
			{
				foreach (var handler in _beforeDownloads)
				{
					handler.Handle(ref request, this, spider);
				}
			}
		}

		private void AfterDownloadComplete(ref Page page, ISpider spider)
		{
			if (_afterDownloadCompletes != null && _afterDownloadCompletes.Count > 0)
			{
				foreach (var handler in _afterDownloadCompletes)
				{
					handler.Handle(ref page, this, spider);
				}
			}
		}

		/// <summary>
		/// Try to detect Content type
		/// </summary>
		/// <param name="page"></param>
		/// <param name="spider"></param>
		private void TryDetectContentType(Page page, ISpider spider)
		{
			if (!_detectedContentType)
			{
				if (page != null && page.Exception == null && spider.Site.ContentType == ContentType.Auto)
				{
					try
					{
						JToken.Parse(page.Content);
						spider.Site.ContentType = ContentType.Json;
					}
					catch
					{
						spider.Site.ContentType = ContentType.Html;
					}
					finally
					{
						_detectedContentType = true;
					}
				}
			}
			if (page != null)
			{
				page.ContentType = spider.Site.ContentType;
			}
		}
	}
}