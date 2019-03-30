using System;
using System.Threading.Tasks;
using DotnetSpider.Downloader.Entity;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Downloader
{
    public class DownloaderAllocator : IDownloaderAllocator
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IDownloaderAgentOptions _options;

        public DownloaderAllocator(
            IDownloaderAgentOptions options,
            ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _options = options;
        }

        public Task<IDownloader> CreateDownloaderAsync(string agentId,
            AllotDownloaderMessage allotDownloaderMessage)
        {
            IDownloader downloader = null;
            switch (allotDownloaderMessage.Type)
            {
                case DownloaderType.Empty:
                {
                    downloader = new EmptyDownloader
                    {
                        AgentId = agentId,
                        Logger = _loggerFactory.CreateLogger<ExceptionDownloader>()
                    };
                    break;
                }
                case DownloaderType.Test:
                {
                    downloader = new TestDownloader
                    {
                        AgentId = agentId,
                        Logger = _loggerFactory.CreateLogger<ExceptionDownloader>()
                    };
                    break;
                }
                case DownloaderType.Exception:
                {
                    downloader = new ExceptionDownloader
                    {
                        AgentId = agentId,
                        Logger = _loggerFactory.CreateLogger<ExceptionDownloader>()
                    };
                    break;
                }
                case DownloaderType.WebDriver:
                {
                    throw new NotImplementedException();
                }
                case DownloaderType.HttpClient:
                {
                    var httpClient = new HttpClientDownloader
                    {
                        AgentId = agentId,
                        UseProxy = allotDownloaderMessage.UseProxy,
                        AllowAutoRedirect = allotDownloaderMessage.AllowAutoRedirect,
                        Timeout = allotDownloaderMessage.Timeout,
                        DecodeHtml = allotDownloaderMessage.DecodeHtml,
                        UseCookies = allotDownloaderMessage.UseCookies,
                        Logger = _loggerFactory.CreateLogger<HttpClientDownloader>(),
                        HttpProxyPool = string.IsNullOrWhiteSpace(_options.ProxySupplyUrl)
                            ? null
                            : new HttpProxyPool(new HttpRowTextProxySupplier(_options.ProxySupplyUrl)),
                        RetryTime = allotDownloaderMessage.RetryTimes
                    };
                    httpClient.AddCookies(allotDownloaderMessage.Cookies);
                    downloader = httpClient;
                    break;
                }
            }

            return Task.FromResult(downloader);
        }
    }
}