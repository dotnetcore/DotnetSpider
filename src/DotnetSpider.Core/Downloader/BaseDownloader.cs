using System;
using System.Collections.Generic;
using NLog;

namespace DotnetSpider.Core.Downloader
{
	public class BaseDownloader : Named, IDownloader, IDisposable
	{
		protected ILogger Logger { get; set; }
		public List<IDownloadCompleteHandler> DownloadCompleteHandlers { get; set; } = new List<IDownloadCompleteHandler>();
		public List<IBeforeDownloadHandler> BeforeDownloadHandlers { get; set; } = new List<IBeforeDownloadHandler>();
		public dynamic Context { get; set; }

		public BaseDownloader()
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

		public virtual Page Download(Request request, ISpider spider)
		{

			return null;
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
