using DotnetSpider.Core;
using Microsoft.Extensions.Hosting;
#if !NET451

#else
using DotnetSpider.Core;
#endif

namespace DotnetSpider.DownloadAgent
{
    /// <summary>
    /// 下载器代理
    /// </summary>
    public interface IDownloaderAgent : IHostedService, IRunnable
    {
    }
}