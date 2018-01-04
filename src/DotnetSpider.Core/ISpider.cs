using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Monitor;
using System;

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
		/// Cookies, 如果需要更换Cookies, 则对此属性赋一个全新的Cookies对象即可(运行中也可以替换)
		/// 爬虫运行中不能通过Cookies.AddCookies等方法再添加新的Cookie
		/// </summary>
		Cookies Cookies { get; set; }

		/// <summary>
		/// 监控接口
		/// </summary>
		IMonitor Monitor { get; set; }
	}
}
