using System;
using System.Threading;
using OpenQA.Selenium.Remote;

namespace DotnetSpider.Extension.Downloader.WebDriverActions
{
	/// <summary>
	/// 滚动操作的实现
	/// </summary>
	public class Scroll : IWebDriverAction
	{
		/// <summary>
		/// 滚动次数
		/// </summary>
		public int ScrollTimes { get; set; } = 1;

		/// <summary>
		/// 滚动操作的具体实现
		/// </summary>
		/// <param name="webDriver">WebDriver</param>
		/// <returns>是否操作成功</returns>
		public bool Invoke(RemoteWebDriver webDriver)
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