using OpenQA.Selenium.Firefox;
using System;
using DotnetSpider.Core.Selector;
using System.Linq;
using System.Threading;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Model;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core;
using System.IO;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Downloader
{

	public class WebDriverCookieInjector : CookieInjector
	{
		public string Url { get; set; }

		public string AfterLoginUrl { get; set; }

		public Selector UserSelector { get; set; }

		public string User { get; set; }

		public Selector PasswordSelector { get; set; }

		public string Password { get; set; }

		public Selector SubmitSelector { get; set; }

		public Browser Browser { get; set; } = Browser.Chrome;

		protected virtual void BeforeInputInfo(RemoteWebDriver webDriver) { }

		protected virtual void AfterLoginComplete(RemoteWebDriver webDriver) { }

		protected IWebElement FindElement(RemoteWebDriver webDriver, Selector element)
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

		protected override Cookies GetCookies(ISpider spider)
		{
			if (string.IsNullOrEmpty(User) || string.IsNullOrEmpty(Password) || UserSelector == null || PasswordSelector == null)
			{
				throw new SpiderException("Arguments of WebDriverCookieInjector are incorrect.");
			}
			var cookies = new Dictionary<string, string>();

			var webDriver = CreateWebDriver();
			try
			{
				webDriver.Navigate().GoToUrl(Url);
				Thread.Sleep(10000);

				BeforeInputInfo(webDriver);

				if (UserSelector != null)
				{
					var user = FindElement(webDriver, UserSelector);
					user.Clear();
					user.SendKeys(User);
					Thread.Sleep(1500);
				}

				if (PasswordSelector != null)
				{
					var pass = FindElement(webDriver, PasswordSelector);
					pass.Clear();
					pass.SendKeys(Password);
					Thread.Sleep(1500);
				}

				var submit = FindElement(webDriver, SubmitSelector);
				submit.Click();
				Thread.Sleep(10000);

				AfterLoginComplete(webDriver);

				var cookieList = webDriver.Manage().Cookies.AllCookies.ToList();
				if (cookieList.Count > 0)
				{
					foreach (var cookieItem in cookieList)
					{
						cookies.Add(cookieItem.Name, cookieItem.Value);
					}
				}

				webDriver.Dispose();
			}
			catch (Exception e)
			{
				Logger.AllLog(spider.Identity, "Get cookie failed.", NLog.LogLevel.Error, e);
				webDriver.Dispose();
				return null;
			}

			var result = new Cookies();
			result.AddCookies(cookies, new Uri(Url).Host);
			return result;
		}

		protected RemoteWebDriver CreateWebDriver()
		{
			RemoteWebDriver webDriver;
			switch (Browser)
			{
				case Browser.Chrome:
					{
						ChromeDriverService cds = ChromeDriverService.CreateDefaultService();
						cds.HideCommandPromptWindow = true;
						ChromeOptions opt = new ChromeOptions();
						opt.AddUserProfilePreference("profile", new { default_content_setting_values = new { images = 2 } });
						webDriver = new ChromeDriver(cds, opt);
						break;
					}
				case Browser.Firefox:
					{
						string path = Environment.ExpandEnvironmentVariables("%APPDATA%") + @"\Mozilla\Firefox\Profiles\";
						string[] pathsToProfiles = Directory.GetDirectories(path, "*.webdriver", SearchOption.TopDirectoryOnly);
						if (pathsToProfiles.Length == 1)
						{
							FirefoxProfile profile = new FirefoxProfile(pathsToProfiles[0], false) { AlwaysLoadNoFocusLibrary = true };
							webDriver = new FirefoxDriver(profile);
						}
						else
						{
							throw new Exception("No Firefox profiles: webdriver.");
						}
						break;
					}
				default:
					{
						throw new Exception("Unsupported browser!");
					}
			}

			webDriver.Manage().Window.Maximize();
			return webDriver;
		}
	}
}
