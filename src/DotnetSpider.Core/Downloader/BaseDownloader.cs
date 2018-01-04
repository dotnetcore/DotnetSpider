using DotnetSpider.Core.Infrastructure;
using Newtonsoft.Json.Linq;
using NLog;
using System.Collections.Generic;
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

		/// <summary>
		/// 重置Cookie
		/// </summary>
		/// <param name="cookies">Cookies</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public virtual void ResetCookies(Cookies cookies) { }

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
					handler.Handle(ref request, spider);
				}
			}
		}

		private void AfterDownloadComplete(ref Page page, ISpider spider)
		{
			if (_afterDownloadCompletes != null && _afterDownloadCompletes.Count > 0)
			{
				foreach (var handler in _afterDownloadCompletes)
				{
					handler.Handle(ref page, spider);
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