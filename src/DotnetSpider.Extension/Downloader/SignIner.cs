#if !NET_CORE
using DotnetSpider.Core;
using DotnetSpider.Extension.Downloader.WebDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Core.Selector;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System.Threading;
using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Downloader
{
	public abstract class SignIner : Named, IWebDriverHandler
	{
		public abstract bool Handle(RemoteWebDriver driver);
	}

	public class CommonSignIner : SignIner
	{
		public string Url { get; set; }

		public Selector UserSelector { get; set; }

		public string User { get; set; }

		public Selector PassSelector { get; set; }

		public string Pass { get; set; }

		public Selector SubmitSelector { get; set; }

		public override bool Handle(RemoteWebDriver webDriver)
		{

			try
			{
				webDriver.Navigate().GoToUrl(Url);
				var user = FindElement(webDriver, UserSelector);

				user.Clear();
				user.SendKeys(User);
				Thread.Sleep(1500);
				var pass = FindElement(webDriver, PassSelector);
				pass.SendKeys(Pass);
				Thread.Sleep(1500);
				var submit = FindElement(webDriver, SubmitSelector);
				submit.Click();
				Thread.Sleep(5000);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private IWebElement FindElement(RemoteWebDriver webDriver, Selector element)
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

	public class ManualSignIner : SignIner
	{
		public string Url { get; set; }

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
#endif