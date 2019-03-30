#if !NET451
using Microsoft.Extensions.Hosting;

#else
using DotnetSpider.Core;
#endif

namespace DotnetSpider.Downloader
{
    /// <summary>
    /// 下载代理器
    /// </summary>
    public interface IDownloaderAgent : IHostedService
    {
    }
}