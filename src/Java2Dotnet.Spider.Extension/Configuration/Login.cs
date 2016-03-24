#if !NET_CORE

using System;
using System.Threading;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public abstract class Loginer
	{
		[Flags]
		public enum Types
		{
			Common
		}

		public abstract Types Type { get; internal set; }


		public abstract bool Login(RemoteWebDriver webDriver);
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

		public override bool Login(RemoteWebDriver webDriver)
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
			throw new SpiderExceptoin("Unsport findy: " + element.Type);
		}
	}
}
#endif