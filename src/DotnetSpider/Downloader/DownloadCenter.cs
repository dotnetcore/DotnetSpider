using DotnetSpider.Core;
using DotnetSpider.MessageQueue;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 下载中心
	/// </summary>
	public class DownloadCenter : DownloadCenterBase
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="mq">消息队列</param>
		/// <param name="downloaderAgentStore">下载器代理存储</param>
		/// <param name="options">系统选项</param>
		/// <param name="logger">日志接口</param>
		public DownloadCenter(IMessageQueue mq, IDownloaderAgentStore downloaderAgentStore, ISpiderOptions options,
			ILogger logger) : base(mq, downloaderAgentStore, options, logger)
		{
		}
	}
}