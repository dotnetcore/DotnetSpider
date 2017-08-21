using DotnetSpider.Core.Infrastructure;
using NLog;
using OpenQA.Selenium.Remote;
using System;
using System.Threading;

namespace DotnetSpider.Extension.Downloader
{
	public class Scroll : IWebDriverHandler
	{
		protected static readonly ILogger Logger = LogCenter.GetLogger();

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