using DotnetSpider.Core.Infrastructure;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// 基础下载器的抽象
	/// </summary>
	public abstract class BaseDownloader : Named, IDownloader
	{
		/// <summary>
		/// 日志接口
		/// </summary>
		protected static readonly ILogger Logger = LogCenter.GetLogger();

		/// <summary>
		/// 是否检测过下载内容的类型
		/// </summary>
		private bool _detectedContentType;
		private readonly List<IAfterDownloadCompleteHandler> _afterDownloadCompletes = new List<IAfterDownloadCompleteHandler>();
		private readonly List<IBeforeDownloadHandler> _beforeDownloads = new List<IBeforeDownloadHandler>();
		protected readonly CookieContainer _cookieContainer = new CookieContainer();

		/// <summary>
		/// Interface to inject cookie.
		/// </summary>
		public ICookieInjector CookieInjector { get; set; }

		/// <summary>
		/// 设置 Cookies
		/// </summary>
		/// <param name="cookiesStr">Cookies的键值对字符串, 如: a1=b;a2=c;</param>
		/// <param name="domain">作用域</param>
		/// <param name="path">作用路径</param>
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
		/// 添加Cookies
		/// </summary>
		/// <param name="cookies">Cookies的键值对</param>
		/// <param name="domain">作用域</param>
		/// <param name="path">作用路径</param>
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
		/// 添加Cookie
		/// </summary>
		/// <param name="name">名称</param>
		/// <param name="value">值</param>
		/// <param name="domain">作用域</param>
		/// <param name="path">作用路径</param>
		public void AddCookie(string name, string value, string domain, string path = "/")
		{
			var cookie = new Cookie(name, value, path, domain);
			AddCookie(cookie);
		}
		
		/// <summary>
		/// Gets a System.Net.CookieCollection that contains the System.Net.Cookie instances that are associated with a specific URI.
		/// </summary>
		/// <param name="uri">The URI of the System.Net.Cookie instances desired.</param>
		/// <returns>A System.Net.CookieCollection that contains the System.Net.Cookie instances that are associated with a specific URI.</returns>
		public CookieCollection GetCookies(Uri uri)
		{
			return _cookieContainer.GetCookies(uri);
		}

		/// <summary>
		/// 设置 Cookie
		/// </summary>
		/// <param name="cookie">Cookie</param>
		public void AddCookie(Cookie cookie)
		{
			if (cookie == null)
			{
				return;
			}
			_cookieContainer.Add(cookie);
			AddCookieToDownloadClient(cookie);
		}

		/// <summary>
		/// 设置 Cookie
		/// </summary>
		/// <param name="cookie">Cookie</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		protected virtual void AddCookieToDownloadClient(Cookie cookie) { }

		/// <summary>
		/// 下载链接内容
		/// </summary>
		/// <param name="request">链接请求</param>
		/// <param name="spider">爬虫接口</param>
		/// <returns>下载内容封装好的页面对象</returns>
		public Page Download(Request request, ISpider spider)
		{
			BeforeDownload(ref request, spider);
			var page = DowloadContent(request, spider);
			AfterDownloadComplete(ref page, spider);
			TryDetectContentType(page, spider);
			return page;
		}

		/// <summary>
		/// 添加处理器
		/// </summary>
		/// <param name="handler"><see cref="IAfterDownloadCompleteHandler"/></param>
		public void AddAfterDownloadCompleteHandler(IAfterDownloadCompleteHandler handler)
		{
			_afterDownloadCompletes.Add(handler);
		}

		/// <summary>
		/// 添加处理器
		/// </summary>
		/// <param name="handler"><see cref="IBeforeDownloadHandler"/></param>
		public void AddBeforeDownloadHandler(IBeforeDownloadHandler handler)
		{
			_beforeDownloads.Add(handler);
		}

		/// <summary>
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
		/// 下载工作的具体实现
		/// </summary>
		/// <param name="request">请求信息</param>
		/// <param name="spider">爬虫</param>
		/// <returns>页面数据</returns>
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

		[MethodImpl(MethodImplOptions.Synchronized)]
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