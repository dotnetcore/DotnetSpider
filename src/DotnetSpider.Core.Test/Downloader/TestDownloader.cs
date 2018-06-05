using DotnetSpider.Core.Downloader;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Test.Downloader
{

	public class TestDownloader : BaseDownloader
	{
		protected override Task<Page> DowloadContent(Request request, ISpider spider)
		{
			var site = spider.Site;
            return Task.FromResult(new Page(request, site.RemoveOutboundLinks ? site.Domains : null)
            {
                Content = "aabbcccdefg下载人数100"
            });
		}
	}
}
