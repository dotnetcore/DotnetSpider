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
		protected bool DetectContentType { get; set; }

		protected List<IAfterDownloadCompleteHandler> AfterDownloadComplete { get; } = new List<IAfterDownloadCompleteHandler>();

		protected List<IBeforeDownloadHandler> BeforeDownload { get; } = new List<IBeforeDownloadHandler>();

		protected string DownloadFolder { get; set; }

		protected BaseDownloader()
		{
			DownloadFolder = Path.Combine(Env.BaseDirectory, "download");
		}

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

		public virtual IDownloader Clone(ISpider spider)
		{
			return (IDownloader)MemberwiseClone();
		}

		public void AddAfterDownloadCompleteHandler(IAfterDownloadCompleteHandler handler)
		{
			AfterDownloadComplete.Add(handler);
		}

		public void AddBeforeDownloadHandler(IBeforeDownloadHandler handler)
		{
			BeforeDownload.Add(handler);
		}

		public virtual void Dispose()
		{
		}

		protected abstract Page DowloadContent(Request request, ISpider spider);

		protected void HandleBeforeDownload(ref Request request, ISpider spider)
		{
			if (BeforeDownload != null && BeforeDownload.Count > 0)
			{
				foreach (var handler in BeforeDownload)
				{
					handler.Handle(ref request, spider);
				}
			}
		}

		protected void HandlerAfterDownloadComplete(ref Page page, ISpider spider)
		{
			if (AfterDownloadComplete != null && AfterDownloadComplete.Count > 0)
			{
				foreach (var handler in AfterDownloadComplete)
				{
					handler.Handle(ref page, spider);
				}
			}
		}

		protected Page SaveFile(Request request, HttpResponseMessage response, ISpider spider)
		{
			var intervalPath = request.Url.LocalPath.Replace("//", "/").Replace("/", Env.PathSeperator);
			string filePath = $"{DownloadFolder}{Env.PathSeperator}{spider.Identity}{intervalPath}";
			if (!File.Exists(filePath))
			{
				try
				{
					string folder = Path.GetDirectoryName(filePath);
					if (!string.IsNullOrEmpty(folder))
					{
						if (!Directory.Exists(folder))
						{
							Directory.CreateDirectory(folder);
						}
					}

					File.WriteAllBytes(filePath, response.Content.ReadAsByteArrayAsync().Result);
				}
				catch (Exception e)
				{
					Logger.MyLog(spider.Identity, "Storage file failed.", LogLevel.Error, e);
				}
			}
			Logger.MyLog(spider.Identity, $"Storage file: {request.Url} success.", LogLevel.Info);
			return new Page(request, null) { SkipRequest = true };
		}

		private void TryDetectContentType(Page page, ISpider spider)
		{
			lock (_lock)
			{
				if (!DetectContentType)
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
							DetectContentType = true;
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
