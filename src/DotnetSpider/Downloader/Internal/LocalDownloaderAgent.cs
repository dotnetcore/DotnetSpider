using DotnetSpider.MessageQueue;
using DotnetSpider.Network;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Downloader.Internal
{
	internal class LocalDownloaderAgent : DownloaderAgentBase
	{
		public LocalDownloaderAgent(IDownloaderAgentOptions options,
			IMessageQueue mq, IDownloaderAllocator downloaderAllocator, NetworkCenter networkCenter,
			ILoggerFactory loggerFactory) : base(options,
			mq, downloaderAllocator, networkCenter, loggerFactory)
		{
			ConfigureDownloader = downloader => downloader.Logger = null;
		}
	}
}