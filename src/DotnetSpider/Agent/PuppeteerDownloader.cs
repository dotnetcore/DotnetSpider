using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.Http;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Agent
{
	public class PuppeteerDownloader : IDownloader
	{
		private ILogger<PuppeteerDownloader> _logger;

		public PuppeteerDownloader(ILogger<PuppeteerDownloader> logger)
		{
			_logger = logger;
		}

		public Task<Response> DownloadAsync(Request request)
		{
			return Task.FromResult(new Response
			{
				RequestHash = request.Hash,
				StatusCode = HttpStatusCode.BadGateway,
				Content = new ResponseContent {Data = Encoding.UTF8.GetBytes("Not impl")}
			});
		}
	}
}
