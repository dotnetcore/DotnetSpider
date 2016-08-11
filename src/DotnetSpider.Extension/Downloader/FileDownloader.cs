using System.IO;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;

namespace DotnetSpider.Extension.Downloader
{
	/// <summary>
	/// Use to do test, so you don't need to download again. 
	/// </summary>
	public class FileDownloader : BaseDownloader
	{
		public IDownloadCompleteHandler DownloadValidation { get; set; }

		public override Page Download(Request request, ISpider spider)
		{
			BeforeDownload(request, spider);
			Page page = new Page(request, spider.Site.ContentType)
			{
				Content = File.ReadAllText(request.Url.LocalPath),
				TargetUrl = request.Url.ToString(),
				Url = request.Url.ToString(),
				StatusCode = 200
			};

			return page;
		}

		public int ThreadNum { get; set; }
	}
}
