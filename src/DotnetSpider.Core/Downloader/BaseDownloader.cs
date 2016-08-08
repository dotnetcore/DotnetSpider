using System;
using System.Collections.Generic;
using DotnetSpider.Core.Common;

using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace DotnetSpider.Core.Downloader
{
	public class BaseDownloader : IDownloader, IDisposable
	{
		protected ILogger Logger { get; set; }
		public List<IDownloadHandler> Handlers { get; set; } = new List<IDownloadHandler>();

		public BaseDownloader()
		{
			Logger = LogManager.GetCurrentClassLogger();
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
			if (Handlers != null)
			{
				foreach (var handler in Handlers)
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
