using System.Threading.Tasks;
using DotnetSpider.Core;

namespace DotnetSpider.Downloader
{
    /// <summary>
    /// 用于测试的下载器，无限抛出异常
    /// </summary>
    internal class ExceptionDownloader : DownloaderBase
    {
        protected override Task<Response> ImplDownloadAsync(Request request)
        {
            throw new SpiderException("From exception downloader");
        }
    }
}