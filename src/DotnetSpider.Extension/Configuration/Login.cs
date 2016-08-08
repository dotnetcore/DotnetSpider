using System;
using System.Threading;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;

#if !NET_CORE
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
#endif

namespace DotnetSpider.Extension.Configuration
{
	public abstract class Loginer
	{
		[Flags]
		public enum Types
		{
			Common,
			Manual,
		}

		public abstract Types Type { get; internal set; }

		public abstract bool Login(dynamic obj);
	}
	
#if !NET_CORE
	public class VerifyCode
	{
		public bool Verify(dynamic webDriver)
		{

			try
			{
				IWebDriver driver = webDriver;
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

	public class CommonLoginer : Loginer
	{
		public override Types Type { get; internal set; } = Types.Common;

		public string Url { get; set; }

		public Selector UserSelector { get; set; }

		public string User { get; set; }

		public Selector PassSelector { get; set; }

		public string Pass { get; set; }

		public Selector SubmitSelector { get; set; }

		public override bool Login(dynamic webDriver)
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

				case ExtractType.XPath:
					{
						return webDriver.FindElementByXPath(element.Expression);
					}
				case ExtractType.Css:
					{
						return webDriver.FindElementByCssSelector(element.Expression);
					}
			}
			throw new SpiderException("Unsport findy: " + element.Type);
		}
	}

	public class ManualLoginer : Loginer
	{
		public override Types Type { get; internal set; } = Types.Manual;

		public string Url { get; set; }

		public override bool Login(dynamic webDriver)
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
#endif
}
