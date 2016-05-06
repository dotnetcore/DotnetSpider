using System;
using System.Linq;
using System.Threading;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Model;
#if !NET_CORE
using OpenQA.Selenium.Chrome;
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


		public abstract string GetCookie();
	}

#if !NET_CORE
	public class CommonCookieTrapper : CookieTrapper
	{
		public override Types Type { get; internal set; } = Types.Common;

		public string Url { get; set; }

		public string AfterLoginUrl { get; set; }

		public Selector UserSelector { get; set; }

		public string User { get; set; }

		public Selector PassSelector { get; set; }

		public string Pass { get; set; }

		public Selector SubmitSelector { get; set; }

		public Selector LoginAreaSelector { get; set; }

		public override string GetCookie()
		{
			string cookie = string.Empty;
			while (string.IsNullOrEmpty(cookie))
			{
				ChromeDriverService cds = ChromeDriverService.CreateDefaultService();
				cds.HideCommandPromptWindow = true;
				ChromeOptions opt = new ChromeOptions();
				opt.AddUserProfilePreference("profile", new { default_content_setting_values = new { images = 2 } });
				RemoteWebDriver webDriver = new ChromeDriver(cds, opt);
				try
				{
					webDriver.Navigate().GoToUrl(Url);
					Thread.Sleep(5000);

					if (LoginAreaSelector != null)
					{
						var loginArea = FindElement(webDriver, LoginAreaSelector);
						loginArea.Click();
						Thread.Sleep(1000);
					}

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

					if (!string.IsNullOrEmpty(AfterLoginUrl))
					{
						webDriver.Navigate().GoToUrl(AfterLoginUrl);
						Thread.Sleep(5000);
					}

					var cookieList = webDriver.Manage().Cookies.AllCookies.ToList();

					if (cookieList.Count > 0)
					{
						foreach (var cookieItem in cookieList)
						{
							cookie += cookieItem.Name + "=" + cookieItem.Value + "; ";
						}
					}

					webDriver.Dispose();
				}
				catch (Exception e)
				{
					webDriver.Dispose();
					cookie = null;
				}
			}

			return cookie;
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
