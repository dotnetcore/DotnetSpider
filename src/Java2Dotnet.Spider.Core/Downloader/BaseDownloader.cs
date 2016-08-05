using System;
using System.Collections.Generic;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Log;
using Java2Dotnet.Spider.Ioc;
using System.Linq;

namespace Java2Dotnet.Spider.Core.Downloader
{
	public class BaseDownloader : IDownloader, IDisposable
	{
		protected ILogService Logger { get; set; }
		public List<IDownloadHandler> DownloadHandlers=new List<IDownloadHandler>();

		public BaseDownloader()
		{
			Logger = ServiceProvider.Get<ILogService>().First();
		}

		public virtual Page Download(Request request, ISpider spider)
		{
			return null;
		}

		public virtual void Dispose()
		{
		}

		protected void Handle(Page page, ISpider spider)
		{
			if (DownloadHandlers != null)
			{
				foreach (var handler in DownloadHandlers)
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
