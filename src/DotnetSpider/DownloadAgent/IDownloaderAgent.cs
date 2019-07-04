
using DotnetSpider.Core;
#if !NET451
using Microsoft.Extensions.Hosting;

#else
using DotnetSpider.Core;
#endif

namespace DotnetSpider.Downloader
{
    /// <summary>
    /// 下载器代理
    /// </summary>
    public interface IDownloaderAgent : IHostedService, IRunnable
    {
    }
}