using System;
using System.Collections.Generic;
using System.IO;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Downloader;

namespace Java2Dotnet.Spider.Extension.Downloader
{
	/// <summary>
	/// Use to do test, so you don't need to download again. 
	/// </summary>
	public class FileDownloader : BaseDownloader
	{
		public IDownloadHandler DownloadValidation { get; set; }

		public override Page Download(Request request, ISpider spider)
		{
			Page page = new Page(request, spider.Site.ContentType);
			page.Content = File.ReadAllText(request.Url.LocalPath);
			page.TargetUrl = request.Url.ToString();
			page.Url = request.Url.ToString();
			page.StatusCode = 200;

			return page;
		}

		public int ThreadNum { get; set; }
	}
}
