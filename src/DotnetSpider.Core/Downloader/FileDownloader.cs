using System.IO;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// 从本地文件中下载内容
	/// </summary>
	public class FileDownloader : BaseDownloader
	{
		protected override Page DowloadContent(Request request, ISpider spider)
		{
			var filePath = request.Uri.AbsoluteUri;

			if (!string.IsNullOrEmpty(filePath))
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