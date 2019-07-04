using DotnetSpider.MessageQueue;
using DotnetSpider.Network;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 下载器代理
	/// </summary>
	public class DefaultDownloaderAgent : DownloaderAgentBase
	{		
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="options">下载器代理选项</param>
		/// <param name="mq">消息队列</param>
		/// <param name="networkCenter">网络中心</param>
		/// <param name="logger">日志接口</param>
		public DefaultDownloaderAgent(DownloaderAgentOptions options, IMessageQueue mq, NetworkCenter networkCenter, ILogger<DefaultDownloaderAgent> logger) : base(options, mq, networkCenter, logger)
		{
		}
	}
}