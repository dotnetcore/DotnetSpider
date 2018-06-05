﻿using System.IO;
using System.Net;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;

namespace DotnetSpider.Extension.Downloader
{
	/// <summary>
	/// Use to do test, so you don't need to download again. 
	/// </summary>
	public class FileDownloader : BaseDownloader
	{
		protected override Task<Page> DowloadContent(Request request, ISpider spider)
		{
			var site = spider.Site;
			request.StatusCode = HttpStatusCode.OK;
			Page page = new Page(request, site.RemoveOutboundLinks ? site.Domains : null)
			{
				Content = File.ReadAllText(request.Url.LocalPath),
				TargetUrl = request.Url.ToString()
			};

			return Task.FromResult(page);
		}
	}
}
