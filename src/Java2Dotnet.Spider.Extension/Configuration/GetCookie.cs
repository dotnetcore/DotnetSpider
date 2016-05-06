using System;
using System.Threading;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Model;

#if !NET_CORE
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
#endif

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public abstract class CookieTrapper
	{
		[Flags]
		public enum Types
		{
			Common
		}

		public abstract Types Type { get; internal set; }


		public abstract string GetCookie(dynamic obj);
	}

#if !NET_CORE
	public class CommonCookieTrapper : CookieTrapper
	{
		public override Types Type { get; internal set; } = Types.Common;

		public string Url { get; set; }

		public Selector UserSelector { get; set; }

		public string User { get; set; }

		public Selector PassSelector { get; set; }

		public string Pass { get; set; }

		public Selector SubmitSelector { get; set; }

		public override string GetCookie(dynamic webDriver)
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

				string cookie = string.Empty;
                var cookieList = webDriver.Manage().Cookies.AllCookies.ToList();

				if (cookieList.Count > 5)
				{
					foreach (var cookieItem in cookieList)
					{
						cookie += cookieItem.Name + "=" + cookieItem.Value + "; ";
					}
				}
				return cookie;
			}
			catch (Exception)
			{
				return "Exception!!!";
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
#endif
}
