using DotnetSpider.Core.Infrastructure;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace DotnetSpider.Core.Downloader
{
	public abstract class BaseDownloader : Named, IDownloader
	{
		protected static readonly ILogger Logger = LogCenter.GetLogger();

		private readonly object _lock = new object();

		/// <summary>
		/// Auto detect content is json or html
		/// </summary>
		private bool _detectContentType;

		private readonly List<IAfterDownloadCompleteHandler> _afterDownloadCompletes = new List<IAfterDownloadCompleteHandler>();

		private readonly List<IBeforeDownloadHandler> _beforeDownloads = new List<IBeforeDownloadHandler>();

		public Page Download(Request request, ISpider spider)
		{
			if (spider.Site == null)
			{
				return null;
			}

			HandleBeforeDownload(ref request, spider);

			var page = DowloadContent(request, spider);

			HandlerAfterDownloadComplete(ref page, spider);

			TryDetectContentType(page, spider);

			return page;
		}

		public void AddAfterDownloadCompleteHandler(IAfterDownloadCompleteHandler handler)
		{
			_afterDownloadCompletes.Add(handler);
		}

		public void AddBeforeDownloadHandler(IBeforeDownloadHandler handler)
		{
			_beforeDownloads.Add(handler);
		}

		public virtual void Dispose()
		{
		}

		protected abstract Page DowloadContent(Request request, ISpider spider);

		private void HandleBeforeDownload(ref Request request, ISpider spider)
		{
			if (_beforeDownloads != null && _beforeDownloads.Count > 0)
			{
				foreach (var handler in _beforeDownloads)
				{
					handler.Handle(ref request, spider);
				}
			}
		}

		private void HandlerAfterDownloadComplete(ref Page page, ISpider spider)
		{
			if (_afterDownloadCompletes != null && _afterDownloadCompletes.Count > 0)
			{
				foreach (var handler in _afterDownloadCompletes)
				{
					handler.Handle(ref page, spider);
				}
			}
		}

		private void TryDetectContentType(Page page, ISpider spider)
		{
			lock (_lock)
			{
				if (!_detectContentType)
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
							_detectContentType = true;
						}
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