using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Http;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Downloader
{
	public class EmptyDownloader : IDownloader
	{
		private int _downloadCount;

		protected ILogger Logger { get; }

		public EmptyDownloader(ILogger<EmptyDownloader> logger)
		{
			Logger = logger;
		}

		public Task<Response> DownloadAsync(Request request)
		{
			Interlocked.Increment(ref _downloadCount);
			if ((_downloadCount % 100) == 0)
			{
				Logger.LogInformation($"download {_downloadCount} already");
			}

			var response = new Response
			{
				RequestHash = request.Hash,
				StatusCode = HttpStatusCode.OK,
				Content = new ByteArrayContent(Encoding.UTF8.GetBytes(""))
			};
			return Task.FromResult(response);
		}

		public string Name => Downloaders.Empty;
	}
}
