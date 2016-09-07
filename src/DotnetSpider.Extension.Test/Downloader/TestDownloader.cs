using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;

namespace DotnetSpider.Extension.Test.Downloader
{
	public class TestDownloader : BaseDownloader
	{
		protected override Page DowloadContent(Request request, ISpider spider)
		{
			return new Page(request, ContentType.Html)
			{
				Content = "aabbcccdefg下载人数100"
			};
		}
	}
}
