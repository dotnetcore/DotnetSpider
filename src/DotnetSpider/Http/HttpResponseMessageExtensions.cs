using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DotnetSpider.Http
{
	public static class HttpResponseMessageExtensions
	{
		public static async Task<Response> ToResponseAsync(this HttpResponseMessage httpResponseMessage)
		{
			var response = new Response {StatusCode = httpResponseMessage.StatusCode};

			foreach (var header in httpResponseMessage.Headers)
			{
				response.Headers.Add(header.Key, header.Value?.ToString());
			}

			response.Version = httpResponseMessage.Version == null
				? HttpVersion.Version11
				: httpResponseMessage.Version;

			response.Headers.TransferEncodingChunked = httpResponseMessage.Headers.TransferEncodingChunked;

			response.Content = new ByteArrayContent(await httpResponseMessage.Content.ReadAsByteArrayAsync());

			foreach (var header in httpResponseMessage.Content.Headers)
			{
				response.Content.Headers.Add(header.Key, header.Value?.ToString());
			}

			return response;
		}
	}
}
