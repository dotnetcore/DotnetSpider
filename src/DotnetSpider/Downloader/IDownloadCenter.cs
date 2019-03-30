#if !NET451
using Microsoft.Extensions.Hosting;

#else
using DotnetSpider.Core;
#endif

namespace DotnetSpider.Downloader
{
    public interface IDownloadCenter : IHostedService
    {
    }
}