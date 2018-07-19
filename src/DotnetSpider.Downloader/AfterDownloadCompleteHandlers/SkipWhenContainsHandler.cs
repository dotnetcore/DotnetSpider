using DotnetSpider.Common;
using System.Linq;

namespace DotnetSpider.Downloader.AfterDownloadCompleteHandlers
{
	/// <summary>
	/// When <see cref="Response"/> contains specified content, this <see cref="Page"/> will be skipped.
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// 当下载的内容包含指定内容时, 直接跳过此链接
	/// </summary>
	public class SkipWhenContainsHandler : AfterDownloadCompleteHandler
	{
		private readonly string[] _contains;

		/// <param name="contains">包含的内容(contents to skip)</param>
		public SkipWhenContainsHandler(params string[] contains)
		{
			_contains = contains;
		}

		/// <summary>
		/// When <see cref="Response"/> contains specified content, this <see cref="Page"/> will be skipped.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 如果页面数据包含指定内容, 跳过当前链接
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
			response.Content = _contains.Any(c => content.Contains(c)) ? null : response.Content;
		}
	}
}
