using DotnetSpider.Core.Infrastructure;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace DotnetSpider.Core.Downloader
{
	public abstract class BaseDownloader : Named, IDownloader
	{
		protected static readonly ILogger Logger = LogCenter.GetLogger();

		protected bool IsDetectedContentType { get; set; }

		protected List<IAfterDownloadCompleteHandler> AfterDownloadComplete { get; set; } = new List<IAfterDownloadCompleteHandler>();

		protected List<IBeforeDownloadHandler> BeforeDownload { get; set; } = new List<IBeforeDownloadHandler>();

		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

		protected string DownloadFolder { get; set; }

		protected BaseDownloader()
		{
			DownloadFolder = Path.Combine(Infrastructure.Environment.BaseDirectory, "data");
		}

		public Page Download(Request request, ISpider spider)
		{
			if (spider.Site == null)
			{
				return null;
			}

			HandleBeforeDownload(ref request, spider);

			var result = DowloadContent(request, spider);

			_lock.EnterWriteLock();
			try
			{
				if (!IsDetectedContentType)
				{
					if (result != null && result.Exception == null && spider.Site.ContentType == ContentType.Auto)
					{
						try
						{
							JToken.Parse(result.Content);
							spider.Site.ContentType = ContentType.Json;
						}
						catch
						{
							spider.Site.ContentType = ContentType.Html;
						}
						finally
						{
							IsDetectedContentType = true;
						}
					}
				}
			}
			finally
			{
				_lock.ExitWriteLock();
			}

			if (result != null)
			{
				result.ContentType = spider.Site.ContentType;
			}

			HandlerAfterDownloadComplete(ref result, spider);

			return result;
		}

		public virtual IDownloader Clone(ISpider spider)
		{
			return (IDownloader)MemberwiseClone();
		}

		public virtual void Dispose()
		{
		}

		public void AddAfterDownloadCompleteHandler(IAfterDownloadCompleteHandler handler)
		{
			AfterDownloadComplete.Add(handler);
		}

		public void AddBeforeDownloadHandler(IBeforeDownloadHandler handler)
		{
			BeforeDownload.Add(handler);
		}

		protected abstract Page DowloadContent(Request request, ISpider spider);

		protected void HandleBeforeDownload(ref Request request, ISpider spider)
		{
			if (BeforeDownload != null && BeforeDownload.Count > 0)
			{
				foreach (var handler in BeforeDownload)
				{
					if (!handler.Handle(ref request, spider))
					{
						break;
					}
				}
			}
		}

		protected void HandlerAfterDownloadComplete(ref Page page, ISpider spider)
		{
			if (AfterDownloadComplete != null && AfterDownloadComplete.Count > 0)
			{
				foreach (var handler in AfterDownloadComplete)
				{
					if (!handler.Handle(ref page, spider))
					{
						break;
					}
				}
			}
		}

		protected Page SaveFile(Request request, HttpResponseMessage response, ISpider spider)
		{
			var intervalPath = request.Url.LocalPath.Replace("//", "/").Replace("/", Infrastructure.Environment.PathSeperator);
			string filePath = $"{DownloadFolder}{Infrastructure.Environment.PathSeperator}{spider.Identity}{intervalPath}";
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
			return new Page(request, null) { IsSkip = true };
		}
	}
}
