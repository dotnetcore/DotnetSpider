using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DotnetSpider.Http;

namespace DotnetSpider.Downloader
{
	public class FileDownloader : IDownloader
	{
		public Task<Response> DownloadAsync(Request request)
		{
			if (Uri.TryCreate(request.Url, UriKind.RelativeOrAbsolute, out Uri uri))
			{
				var file = uri.AbsoluteUri.Replace("file://", "");

				var response = new Response {RequestHash = request.Hash};
				if (!File.Exists(file))
				{
					response.StatusCode = HttpStatusCode.NotFound;
				}

				var stopwatch = new Stopwatch();
				stopwatch.Start();
				response.TargetUrl = request.Url;
				response.Content = new ResponseContent {Data = File.ReadAllBytes(file)};
				stopwatch.Stop();
				response.StatusCode = HttpStatusCode.OK;
				response.ElapsedMilliseconds = (int)stopwatch.ElapsedMilliseconds;
				return Task.FromResult(response);
			}

			return Task.FromResult(new Response {StatusCode = HttpStatusCode.NotFound, RequestHash = request.Hash});
		}

		public string Name => DownloaderNames.File;
	}
}
