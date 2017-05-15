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
			Page page = new Page(request, spider.Site.ContentType, site.RemoveOutboundLinks ? site.Domains : null)
			{
				Content = File.ReadAllText(request.Url.LocalPath),
				TargetUrl = request.Url.ToString(),
				StatusCode = HttpStatusCode.OK
			};

			return page;
		}
	}
}
