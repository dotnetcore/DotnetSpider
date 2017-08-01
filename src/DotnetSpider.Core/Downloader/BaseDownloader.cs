using DotnetSpider.Core.Infrastructure;
using NLog;
using System;
using System.IO;
using System.Net.Http;

namespace DotnetSpider.Core.Downloader
{
	public abstract class BaseDownloader : Named, IDownloader, IDisposable
	{
		protected readonly static ILogger Logger = LogCenter.GetLogger();
		public IDownloadCompleteHandler[] DownloadCompleteHandlers { get; set; }
		public IBeforeDownloadHandler[] BeforeDownloadHandlers { get; set; }

		protected string DownloadFolder { get; set; }

		protected BaseDownloader()
		{
#if !NET_CORE
			DownloadFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
#else
			DownloadFolder = Path.Combine(AppContext.BaseDirectory, "data");
#endif
		}

		public Page Download(Request request, ISpider spider)
		{
			if (spider.Site == null)
			{
				return null;
			}

			BeforeDownload(request, spider);

			var result = DowloadContent(request, spider);

			AfterDownloadComplete(ref result, spider);

			return result;
		}

		public virtual IDownloader Clone()
		{
			return (IDownloader)MemberwiseClone();
		}

		public virtual void Dispose()
		{
		}

		protected abstract Page DowloadContent(Request request, ISpider spider);

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

		protected void AfterDownloadComplete(ref Page page, ISpider spider)
		{
			if (DownloadCompleteHandlers != null)
			{
				foreach (var handler in DownloadCompleteHandlers)
				{
					var success = handler.Handle(ref page, spider);
					if (!success)
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
					if (!Directory.Exists(folder))
					{
						Directory.CreateDirectory(folder);
					}
					File.WriteAllBytes(filePath, response.Content.ReadAsByteArrayAsync().Result);
				}
				catch (Exception e)
				{
					Logger.MyLog(spider.Identity, "保存文件失败。", LogLevel.Error, e);
				}
			}
			Logger.MyLog(spider.Identity, $"下载文件: {request.Url} 成功.", LogLevel.Info);
			return new Page(request, ContentType.File, null) { IsSkip = true };
		}
	}
}
