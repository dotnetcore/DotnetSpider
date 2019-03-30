using DotnetSpider.MessageQueue;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Downloader.Internal
{
    internal class LocalDownloaderAgent : DownloaderAgentBase
    {
        public LocalDownloaderAgent(IDownloaderAgentOptions options, IMessageQueue mq, IDownloaderAllocator downloaderAllocator, ILoggerFactory loggerFactory) : base(options, mq, downloaderAllocator, loggerFactory)
        {
            ConfigureDownloader = downloader => downloader.Logger = null;
        }
    }
}