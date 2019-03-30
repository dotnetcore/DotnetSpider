using DotnetSpider.MessageQueue;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Downloader.Internal
{
    public class LocalDownloadCenter : DownloadCenterBase
    {
        public LocalDownloadCenter(IMessageQueue mq,
            IDownloaderAgentStore downloaderAgentStore,
            ILogger<LocalDownloadCenter> logger) : base(mq, downloaderAgentStore, logger)
        {
        }       
    }
}