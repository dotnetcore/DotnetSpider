using System;
using System.Collections.Generic;
using NLog;

namespace DotnetSpider.Core.Downloader
{
	public abstract class BaseDownloader : Named, IDownloader, IDisposable
	{
		protected ILogger Logger { get; set; }
		public IDownloadCompleteHandler[] DownloadCompleteHandlers { get; set; }  
		public  IBeforeDownloadHandler[] BeforeDownloadHandlers { get; set; }  
		public dynamic Context { get; set; }

		protected abstract Page DowloadContent(Request request, ISpider spider);

		protected BaseDownloader()
		{
			Logger = LogManager.GetCurrentClassLogger();
		}

		protected void BeforeDownload(Request request, ISpider spider)
		{
			if (BeforeDownloadHandlers != null)
			{
				foreach (var handler in BeforeDownloadHandlers)
				{
					handler.Handle(request, spider);
				}
			}
		}

		public Page Download(Request request, ISpider spider)
		{
			if (spider.Site == null)
			{
				return null;
			}

			BeforeDownload(request, spider);

			var result = DowloadContent(request, spider);

			AfterDownloadComplete(result, spider);

			if (result.Exception != null)
			{
				throw result.Exception;
			}

			return result;
		}

		public virtual void Dispose()
		{
		}

		protected void AfterDownloadComplete(Page page, ISpider spider)
		{
			if (DownloadCompleteHandlers != null)
			{
				foreach (var handler in DownloadCompleteHandlers)
				{
					handler.Handle(page);
				}
			}
		}

		public virtual IDownloader Clone()
		{
			return (IDownloader)MemberwiseClone();
		}
	}
}
