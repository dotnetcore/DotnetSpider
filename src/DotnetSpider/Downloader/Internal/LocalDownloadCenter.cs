using DotnetSpider.MessageQueue;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Downloader.Internal
{
	/// <summary>
	/// 本地下载中心
	/// </summary>
    public class LocalDownloadCenter : DownloadCenterBase
    {
	    /// <summary>
	    /// 构造方法
	    /// </summary>
	    /// <param name="mq">消息队列</param>
	    /// <param name="downloaderAgentStore">下载器代理存储</param>
	    /// <param name="logger">日志接口</param>
        public LocalDownloadCenter(IMessageQueue mq,
            IDownloaderAgentStore downloaderAgentStore,
            ILogger<LocalDownloadCenter> logger) : base(mq, downloaderAgentStore, logger)
        {
        }       
    }
}