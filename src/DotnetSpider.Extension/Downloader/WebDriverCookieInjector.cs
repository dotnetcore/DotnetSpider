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
using System.Net;

namespace DotnetSpider.Extension.Downloader
{
	/// <summary>
	/// WebDriver 的Cookie注入器
	/// </summary>
	public class WebDriverCookieInjector : CookieInjector
	{
		/// <summary>
		/// 登陆的链接
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// 登陆成功后需要再次导航到的链接
		/// </summary>
		public string AfterLoginUrl { get; set; }

		/// <summary>
		/// 用户名在网页中的元素选择器
		/// </summary>
		public Selector UserSelector { get; set; }

		/// <summary>
		/// 用户名
		/// </summary>
		public string User { get; set; }

		/// <summary>
		/// 密码在网页中的元素选择器
		/// </summary>
		public Selector PasswordSelector { get; set; }

		/// <summary>
		/// 密码
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// 登陆按钮的元素选择器
		/// </summary>
		public Selector SubmitSelector { get; set; }

		/// <summary>
		/// 浏览器
		/// </summary>
		public Browser Browser { get; set; } = Browser.Chrome;

		/// <summary>
		/// 在输入用户信息前执行的一些准备操作
		/// </summary>
		/// <param name="webDriver">WebDriver</param>
		protected virtual void BeforeInput(RemoteWebDriver webDriver) { }

		/// <summary>
		/// 完成登陆后执行的一些准备操作
		/// </summary>
		/// <param name="webDriver"></param>
		protected virtual void AfterLogin(RemoteWebDriver webDriver) { }

		/// <summary>
		/// 查找元素
		/// </summary>
		/// <param name="webDriver">WebDriver</param>
		/// <param name="element">页面元素选择器</param>
		/// <returns>页面元素</returns>
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

		/// <summary>
		/// 取得 Cookie
		/// </summary>
		/// <param name="spider">爬虫</param>
		/// <returns>Cookie</returns>
		protected override CookieCollection GetCookies(ISpider spider)
		{
			if (string.IsNullOrEmpty(User) || string.IsNullOrEmpty(Password) || UserSelector == null || PasswordSelector == null)
			{
				throw new SpiderException("Arguments of WebDriverCookieInjector are incorrect");
			}
			var cookies = new Dictionary<string, string>();

			var webDriver = CreateWebDriver();
			var result = new CookieCollection();
			try
			{
				webDriver.Navigate().GoToUrl(Url);
				Thread.Sleep(10000);

				BeforeInput(webDriver);

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

				AfterLogin(webDriver);

				var cookieList = webDriver.Manage().Cookies.AllCookies.ToList();
				if (cookieList.Count > 0)
				{
					foreach (var cookieItem in cookieList)
					{
						result.Add(new System.Net.Cookie(cookieItem.Name, cookieItem.Value, cookieItem.Path, cookieItem.Domain));
					}
				}

				webDriver.Dispose();
			}
			catch
			{
				spider.Logger.Error("Get cookie failed.");
				webDriver.Dispose();
				return null;
			}

			return result;
		}

		/// <summary>
		/// 创建WebDriver对象
		/// </summary>
		/// <returns>WebDriver对象</returns>
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
							FirefoxOptions options = new FirefoxOptions();
							options.Profile = profile;
							webDriver = new FirefoxDriver(options);
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
