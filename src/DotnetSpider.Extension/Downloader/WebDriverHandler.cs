using DotnetSpider.Core.Infrastructure;
using OpenQA.Selenium.Remote;
using System;
using System.Threading;

namespace DotnetSpider.Extension.Downloader
{
	/// <summary>
	/// 滚动操作的实现
	/// </summary>
	public class Scroll : IWebDriverHandler
	{
		/// <summary>
		/// 日志接口
		/// </summary>
		protected static readonly ILogger Logger = DLog.GetLogger();

		/// <summary>
		/// 滚动次数
		/// </summary>
		public int ScrollTimes { get; set; } = 1;

		/// <summary>
		/// 滚动操作的具体实现
		/// </summary>
		/// <param name="webDriver">WebDriver</param>
		/// <returns>是否操作成功</returns>
		public bool Handle(RemoteWebDriver webDriver)
		{
			try
			{
				webDriver.Manage().Window.Maximize();
				for (int i = 0; i <= ScrollTimes; i++)
				{
					webDriver.ExecuteScript("window.scrollBy(0, 500)");
					Thread.Sleep(1000);
				}
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}
	}
}