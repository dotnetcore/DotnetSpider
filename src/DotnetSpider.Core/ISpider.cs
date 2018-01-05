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

		/// <summary>
		/// 设置 Cookie
		/// </summary>
		/// <param name="cookie">Cookie</param>
		void AddCookie(Cookie cookie);

		/// <summary>
		/// 设置 Cookies
		/// </summary>
		/// <param name="cookiesStr">Cookies的键值对字符串, 如: a1=b;a2=c;</param>
		/// <param name="domain">作用域</param>
		/// <param name="path">作用路径</param>
		void AddCookies(string cookiesStr, string domain, string path = "/");
	}
}
