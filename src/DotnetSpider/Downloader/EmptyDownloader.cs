using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpider.Downloader
{
	public class EmptyDownloader : DownloaderBase
	{
		protected override Task<Response> ImplDownloadAsync(Request request)
		{
			Thread.Sleep(150);
			return Task.FromResult(new Response
			{
				Request = request,
				Content = Encoding.UTF8.GetBytes("From empty downloader"),
				CharSet = "UTF-8",
				Success = true,
				TargetUrl = request.Url
			});
		}
	}
}
