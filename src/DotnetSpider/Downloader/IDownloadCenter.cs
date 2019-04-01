#if !NET451
using Microsoft.Extensions.Hosting;

#else
using DotnetSpider.Core;
#endif

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 下载中心
	/// </summary>
    public interface IDownloadCenter : IHostedService
    {
    }
}