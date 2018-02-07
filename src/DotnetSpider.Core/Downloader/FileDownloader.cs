using DotnetSpider.Core.Infrastructure;
using System;
using System.IO;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// Download from local filesystem.
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// 从本地文件中下载内容
	/// </summary>
	public class FileDownloader : BaseDownloader
	{
		/// <summary>
		/// Download from local filesystem.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 从本地文件中下载内容
		/// </summary>
		/// <param name="request">请求信息 <see cref="Request"/></param>
		/// <param name="spider">爬虫 <see cref="ISpider"/></param>
		/// <returns>页面数据 <see cref="Page"/></returns>
		protected override Page DowloadContent(Request request, ISpider spider)
		{
			var filePath = request.Uri.LocalPath;
			if (filePath.StartsWith("\\"))
			{
				filePath = filePath.Substring(2, filePath.Length - 2);
			}
			if (!string.IsNullOrWhiteSpace(filePath))
			{
				if (!filePath.Contains(":"))
				{
					filePath = Path.Combine(Env.BaseDirectory, filePath);
				}
				if (File.Exists(filePath))
				{
					return new Page(request)
					{
						Content = File.ReadAllText(filePath)
					};
				}
			}
			var msg = $"File {filePath} unfound.";
			Page page = new Page(request)
			{
				Exception = new DownloadException(msg),
				Skip = true
			};

			Logger.Log(spider.Identity, $"Download {request.Url} failed: {msg}.", Level.Error);
			return page;
		}
	}
}