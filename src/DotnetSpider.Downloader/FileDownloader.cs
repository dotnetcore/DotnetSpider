using System;
using System.IO;
using System.Net;
using DotnetSpider.Common;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// Download from local filesystem.
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// 从本地文件中下载内容
	/// </summary>
	public class FileDownloader : Downloader
	{
		/// <summary>
		/// Download from local filesystem.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 从本地文件中下载内容
		/// </summary>
		/// <param name="request">请求信息 <see cref="Request"/></param>
		/// <returns>页面数据 <see cref="Response"/></returns>
		protected override Response DowloadContent(Request request)
		{
			Console.WriteLine(request.Url);
			var filePath = new Uri(request.Url).LocalPath;
			if (!DownloaderEnv.IsWindows)
			{
				filePath = filePath.Replace("\\", "/");
			}
			if (filePath.StartsWith("\\"))
			{
				filePath = filePath.Substring(2, filePath.Length - 2);
			}
			if (!string.IsNullOrWhiteSpace(filePath))
			{
				if (DownloaderEnv.IsWindows && !filePath.Contains(":"))
				{
					filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
				}
				if (File.Exists(filePath))
				{
					return new Response { Request = request, Content = File.ReadAllText(filePath), StatusCode = HttpStatusCode.OK, TargetUrl = request.Url, ContentType = request.Site.ContentType };
				}
			}
			var msg = $"File {filePath} unfound.";
			var response = new Response { Request = request, Content = null, StatusCode = HttpStatusCode.NotFound, ContentType = request.Site.ContentType, TargetUrl = request.Url };

			Logger.Error($"Download {request.Url} failed: {msg}.");
			return response;
		}
	}
}