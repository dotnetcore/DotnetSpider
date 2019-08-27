using DotnetSpider.Common;
using DotnetSpider.DownloadAgentRegisterCenter;
using DotnetSpider.MessageQueue;
using Microsoft.Extensions.Logging;

namespace DotnetSpider
{
	/// <summary>
	/// 下载中心
	/// </summary>
	public class DefaultDownloadAgentRegisterCenter : DownloadAgentRegisterCenterBase
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="mq">消息队列</param>
		/// <param name="downloaderAgentStore">下载器代理存储</param>
		/// <param name="options">系统选项</param>
		/// <param name="logger">日志接口</param>
		public DefaultDownloadAgentRegisterCenter(IMq mq, IDownloaderAgentStore downloaderAgentStore, SpiderOptions options,
			ILogger<DefaultDownloadAgentRegisterCenter> logger) : base(mq, downloaderAgentStore, options, logger)
		{
		}
	}
}
