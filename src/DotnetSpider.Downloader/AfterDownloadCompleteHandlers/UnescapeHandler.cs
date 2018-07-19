using DotnetSpider.Common;
using System.Text.RegularExpressions;

namespace DotnetSpider.Downloader.AfterDownloadCompleteHandlers
{
	/// <summary>
	/// Converts any escaped characters in current content.
	/// </summary>
	public class UnescapeHandler : AfterDownloadCompleteHandler
	{
		/// <summary>
		/// Converts any escaped characters in current content.
		/// </summary>
		/// <param name="response">页面数据 <see cref="Response"/></param>
		/// <param name="downloader">下载器 <see cref="IDownloader"/></param>
		public override void Handle(ref Response response, IDownloader downloader)
		{
			if (response == null || string.IsNullOrWhiteSpace(response.Content))
			{
				return;
			}
			response.Content = Regex.Unescape(response.Content);
		}
	}
}
