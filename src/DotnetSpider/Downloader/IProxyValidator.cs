using System.Net;

namespace DotnetSpider.Downloader
{
    public interface IProxyValidator
    {
        bool IsAvailable(WebProxy proxy);
    }
}