using System.Net.Http;

namespace DotnetSpider.Downloader
{
	public class HttpClientEntry
	{
		public HttpClient HttpClient { get; }
		public dynamic Resource { get; }

		public HttpClientEntry(HttpClient httpClient, dynamic resource)
		{
			HttpClient = httpClient;
			Resource = resource;
		}
	}
}
