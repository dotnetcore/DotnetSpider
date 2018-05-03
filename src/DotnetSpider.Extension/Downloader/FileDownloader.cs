using System.IO;
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
		/// <summary>
		/// 从文件中读取内容
		/// </summary>
		/// <param name="request">请求信息</param>
		/// <param name="spider">爬虫</param>
		/// <returns></returns>
		protected override Task<Page> DowloadContent(Request request, ISpider spider)
		{
			var site = spider.Site;
			request.StatusCode = HttpStatusCode.OK;
			Page page = new Page(request)
			{
				Content = File.ReadAllText(request.Uri.LocalPath),
				TargetUrl = request.Url.ToString()
			};

			return Task.FromResult(page);
		}
	}
}
