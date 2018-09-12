using DotnetSpider.Common;
using System;
using System.Net;
using System.Threading;

namespace DotnetSpider.Extension.Downloader
{
	/// <summary>
	/// 手动登录操作的实现
	/// </summary>
	public class ManualWebDriverCookieInjector : WebDriverCookieInjector
	{
		/// <summary>
		/// 登陆的链接
		/// </summary>
		private readonly string _url;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="url">登陆的链接</param>
		/// <param name="browser">浏览器</param>
		public ManualWebDriverCookieInjector(string url, Browser browser, Action before = null, Action after = null) : base(browser, before, after)
		{
			_url = url;
		}

		protected override CookieCollection GetCookies()
		{
			var driver = CreateWebDriver();
			try
			{
				driver.Navigate().GoToUrl(_url);
				while (!driver.Url.Contains("baidu.com"))
				{
					Thread.Sleep(1000);
				}

				return driver.GetCookieCollection();
			}
			finally
			{
				driver.Dispose();
			}
		}
	}
}
