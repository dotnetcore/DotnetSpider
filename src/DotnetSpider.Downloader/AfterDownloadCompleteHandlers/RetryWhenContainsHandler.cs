using DotnetSpider.Common;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace DotnetSpider.Downloader.AfterDownloadCompleteHandlers
{
	/// <summary>
	/// Retry current link when <see cref="Response"/> contains specified contents.
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// 当包含指定内容时重试当前链接
	/// </summary>
	public class RetryWhenContainsHandler : AfterDownloadCompleteHandler
	{
		private readonly string[] _contents;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 构造方法
		/// </summary>
		/// <param name="contents">包含的内容(specified contents to detect.)</param>
		public RetryWhenContainsHandler(params string[] contents)
		{
			if (contents == null || contents.Length == 0)
			{
				throw new ArgumentException("contents is null.");
			}

			_contents = contents;
		}

		/// <summary>
		/// Retry current link when <see cref="Response"/> contains specified contents.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 当包含指定内容时重试当前链接
		/// </summary>
		/// <param name="response">页面数据 <see cref="Response"/></param>
		/// <param name="downloader">下载器 <see cref="IDownloader"/></param>
		public override void Handle(ref Response response, IDownloader downloader)
		{
			if (response == null || string.IsNullOrWhiteSpace(response.Content))
			{
				return;
			}
			var content = response.Content;
			if (_contents.Any(c => content.Contains(c)))
			{
				throw new DownloaderException($"Retry this request because content contains {JsonConvert.SerializeObject(_contents)}.");
			}
		}
	}
}
