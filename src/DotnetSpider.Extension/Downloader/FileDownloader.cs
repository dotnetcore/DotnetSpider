using System;
using System.IO;
using System.Net;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;

namespace DotnetSpider.Extension.Downloader
{
	/// <summary>
	/// Use to do test, so you don't need to download again. 
	/// </summary>
	public class FileDownloader : BaseDownloader
	{
		protected override Page DowloadContent(Request request, ISpider spider)
		{
			var site = spider.Site;
			request.StatusCode = HttpStatusCode.OK;
			Page page = new Page(request)
			{
				Content = File.ReadAllText(request.Uri.LocalPath),
				TargetUrl = request.Url.ToString()
			};

			return page;
		}
	}
}
