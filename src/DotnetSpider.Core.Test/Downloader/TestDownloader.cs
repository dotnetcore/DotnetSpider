using DotnetSpider.Core.Downloader;

namespace DotnetSpider.Core.Test.Downloader
{

	public class TestDownloader : BaseDownloader
	{
		protected override Page DowloadContent(Request request, ISpider spider)
		{
			var site = spider.Site;
			return new Page(request)
			{
				Content = "aabbcccdefg下载人数100"
			};
		}
	}
}
