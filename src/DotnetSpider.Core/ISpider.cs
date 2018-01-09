using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Monitor;
using System;
using System.Net;

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

		/// <summary>
		/// 监控接口
		/// </summary>
		IMonitor Monitor { get; set; }
	}
}
