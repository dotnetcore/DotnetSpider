using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Monitor;
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

		/// <summary>
		/// Cookies
		/// </summary>
		Cookies Cookies { get; set; }

		/// <summary>
		/// 监控接口
		/// </summary>
		IMonitor Monitor { get; set; }
	}
}
