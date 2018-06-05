﻿using DotnetSpider.Core;
using System;
using DotnetSpider.Core.Selector;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System.Threading;
using DotnetSpider.Extension.Model;
using DotnetSpider.Core.Infrastructure;
using NLog;

namespace DotnetSpider.Extension.Downloader
{
	public abstract class LoginHandler : Named, IWebDriverHandler
	{
		protected static readonly ILogger Logger = LogCenter.GetLogger();

		public abstract bool Handle(RemoteWebDriver driver);
	}

	public class CommonLoginHandler : LoginHandler
	{
		public string Url { get; set; }

		public SelectorAttribute UserSelector { get; set; }

		public string User { get; set; }

		public SelectorAttribute PassSelector { get; set; }

		public string Password { get; set; }

		public SelectorAttribute SubmitSelector { get; set; }

		public override bool Handle(RemoteWebDriver webDriver)
		{
			try
			{
				webDriver.Navigate().GoToUrl(Url);
				Thread.Sleep(5000);

				var user = FindElement(webDriver, UserSelector);
				user.Clear();
				user.SendKeys(User);
				Thread.Sleep(1000);

				var password = FindElement(webDriver, PassSelector);
				password.Clear();
				password.SendKeys(Password);
				Thread.Sleep(1000);

				var submit = FindElement(webDriver, SubmitSelector);
				submit.Click();
				Thread.Sleep(5000);

				return true;
			}
			catch (Exception ex)
			{
				Logger.AllLog($"LoginHandler failed: {ex}.", NLog.LogLevel.Error);
				return false;
			}
		}

		private IWebElement FindElement(RemoteWebDriver webDriver, SelectorAttribute element)
		{
			switch (element.Type)
			{

				case SelectorType.XPath:
					{
						return webDriver.FindElementByXPath(element.Expression);
					}
				case SelectorType.Css:
					{
						return webDriver.FindElementByCssSelector(element.Expression);
					}
			}
			throw new SpiderException("Unsport findy: " + element.Type);
		}
	}

	public class ManualLoginHandler : LoginHandler
	{
		public Uri Url { get; set; }

		public ManualLoginHandler(string url)
		{
			Url = new Uri(url);
		}

		public override bool Handle(RemoteWebDriver webDriver)
		{
			try
			{
				IWebDriver driver = webDriver;
				driver.Navigate().GoToUrl(Url);
				while (!driver.Url.Contains("baidu.com"))
				{
					Thread.Sleep(1000);
				}
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
