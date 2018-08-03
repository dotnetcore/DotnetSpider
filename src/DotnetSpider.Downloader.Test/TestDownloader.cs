using DotnetSpider.Common;

namespace DotnetSpider.Downloader.Test
{
	public class TestDownloader : Downloader
	{
		protected override Response DowloadContent(Request request)
		{
			return new Response() { Request = request, Content = "aabbcccdefg下载人数100" };
		}
	}
}
