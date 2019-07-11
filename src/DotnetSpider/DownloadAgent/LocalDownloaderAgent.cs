using DotnetSpider.Common;
using DotnetSpider.EventBus;
using DotnetSpider.Network;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.DownloadAgent
{
	/// <summary>
	/// 本地下器代理
	/// </summary>
	public class LocalDownloaderAgent : DefaultDownloaderAgent
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="options">下载器代理选项</param>
		/// <param name="spiderOptions"></param>
		/// <param name="eventBus">消息队列</param>
		/// <param name="networkCenter">网络中心</param>
		/// <param name="logger">日志接口</param>
		public LocalDownloaderAgent(DownloaderAgentOptions options, SpiderOptions spiderOptions,
			IEventBus eventBus, NetworkCenter networkCenter,
			ILogger<LocalDownloaderAgent> logger) : base(options, spiderOptions,
			eventBus, networkCenter, logger)
		{
			// ConfigureDownloader = downloader => downloader.Logger = null;
		}
	}
}