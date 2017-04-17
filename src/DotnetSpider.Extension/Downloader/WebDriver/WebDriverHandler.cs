#if !NET_CORE
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.Downloader.WebDriver
{
	public class ScrollHandler : IWebDriverHandler
	{
		public int Interval { get; set; } = 1;
		public int ScrollTo { get; set; } = 0;

		public bool Handle(RemoteWebDriver webDriver)
		{
			try
			{
				webDriver.Manage().Window.Maximize();
				for (int i = 0; i <= Interval; i++)
				{
					webDriver.ExecuteScript($"window.scrollBy(0, {ScrollTo})");
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
#endif