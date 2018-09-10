using DotnetSpider.Common;
using DotnetSpider.Downloader;
using HtmlAgilityPack;

namespace DotnetSpider.Core.Downloader.AfterDownloadCompleteHandlers
{
	/// <summary>
	/// Handler that removes HTML tags in <see cref="Page.Content"/>.
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// 去除下载内容中的HTML标签
	/// </summary>
	public class PlainTextHandler : AfterDownloadCompleteHandler
	{
		/// <summary>
		/// Remove HTML tags in <see cref="Page.Content"/>.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 去除下载内容中的HTML标签
		/// </summary>
		/// <param name="response">页面数据 <see cref="Response"/></param>
		/// <param name="downloader">下载器 <see cref="IDownloader"/></param>
		public override void Handle(ref Response response, IDownloader downloader)
		{
			var text = response?.Content?.ToString();
			if (string.IsNullOrWhiteSpace(text))
			{
				return;
			}
			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(text);
			response.Content = htmlDocument.DocumentNode.InnerText;
		}
	}
}
