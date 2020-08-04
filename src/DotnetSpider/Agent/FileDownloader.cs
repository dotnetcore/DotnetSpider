using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DotnetSpider.Http;

namespace DotnetSpider.Agent
{
	public class FileDownloader : IDownloader
	{
		public Task<Response> DownloadAsync(Request request)
		{
			var file = request.RequestUri.AbsoluteUri.Replace("file://", "");
			var response = new Response {RequestHash = request.Hash};
			if (!File.Exists(file))
			{
				response.StatusCode = HttpStatusCode.NotFound;
			}

			var stopwatch = new Stopwatch();
			stopwatch.Start();
			response.TargetUri = request.RequestUri.ToString();
			response.Content = new ResponseContent {Data = File.ReadAllBytes(file)};
			stopwatch.Stop();
			response.StatusCode = HttpStatusCode.OK;
			response.ElapsedMilliseconds = (int)stopwatch.ElapsedMilliseconds;
			return Task.FromResult(response);
		}
	}
}
