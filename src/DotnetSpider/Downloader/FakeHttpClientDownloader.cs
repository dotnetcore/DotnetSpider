using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.Proxy;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Downloader;

public class FakeHttpClientDownloader(
    IHttpClientFactory httpClientFactory,
    IProxyService proxyService,
    ILogger<HttpClientDownloader> logger)
    : HttpClientDownloader(httpClientFactory, proxyService, logger)
{
    protected override Task<HttpResponseMessage> SendAsync(HttpClient httpClient,
        HttpRequestMessage httpRequestMessage)
    {
        return Task.FromResult(new HttpResponseMessage
        {
            Content = new StringContent("<html></html>", Encoding.UTF8),
            RequestMessage = httpRequestMessage,
            StatusCode = HttpStatusCode.OK,
            Version = HttpVersion.Version11
        });
    }
}
