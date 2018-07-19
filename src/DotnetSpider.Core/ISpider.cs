using DotnetSpider.Common;
using DotnetSpider.Core.Monitor;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Downloader;
using System;
using System.Collections.Generic;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 爬虫接口定义
	/// </summary>
	public interface ISpider : IDisposable, IControllable, IAppBase
	{
		/// <summary>
		/// 采集站点的信息配置
		/// </summary>
		Site Site { get; }

		IScheduler Scheduler { get; }

		/// <summary>
		/// 下载器
		/// </summary>
		IDownloader Downloader { get; set; }

		/// <summary>
		/// 监控接口
		/// </summary>
		IMonitor Monitor { get; set; }
	}
}
