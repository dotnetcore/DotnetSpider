using System.IO;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// 从本地文件中下载内容
	/// </summary>
	public class FileDownloader : BaseDownloader
	{
		/// <summary>
		/// 从本地文件中下载内容
		/// </summary>
		/// <param name="request">请求信息</param>
		/// <param name="spider">爬虫</param>
		/// <returns>页面数据</returns>
		protected override Page DowloadContent(Request request, ISpider spider)
		{
			var filePath = request.Uri.AbsoluteUri;

			if (!string.IsNullOrWhiteSpace(filePath))
			{
				if (File.Exists(filePath))
				{
					return new Page(request)
					{
						Content = File.ReadAllText(filePath)
					};
				}
			}

			return null;
		}
	}
}