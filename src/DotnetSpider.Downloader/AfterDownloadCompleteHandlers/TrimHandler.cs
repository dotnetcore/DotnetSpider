using DotnetSpider.Common;

namespace DotnetSpider.Downloader.AfterDownloadCompleteHandlers
{
	/// <summary>
	/// Removes all leading and trailing white-space characters from the current content.
	/// </summary>
	public class TrimHandler : AfterDownloadCompleteHandler
	{
		/// <summary>
		/// Removes all leading and trailing white-space characters from the current content.
		/// </summary>
		/// <param name="response">页面数据 <see cref="Response"/></param>
		/// <param name="downloader">下载器 <see cref="IDownloader"/></param>
		public override void Handle(ref Response response, IDownloader downloader)
		{
			if (response == null || string.IsNullOrWhiteSpace(response.Content))
			{
				return;
			}
			response.Content = response.Content.Trim();
		}
	}
}
