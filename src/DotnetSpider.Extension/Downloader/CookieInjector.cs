#if !NET_CORE
using OpenQA.Selenium.Firefox;
using System;
using DotnetSpider.Core.Selector;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Model;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core;
using System.IO;
using DotnetSpider.Extension.Infrastructure;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Downloader
{

	public abstract class WebDriverCookieInjector : CookieInjector
	{
		public Browser Browser { get; set; } = Browser.Chrome;

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

		protected RemoteWebDriver GetWebDriver()
		{
			RemoteWebDriver webDriver = null;
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
						string path = System.Environment.ExpandEnvironmentVariables("%APPDATA%") + @"\Mozilla\Firefox\Profiles\";
						string[] pathsToProfiles = Directory.GetDirectories(path, "*.webdriver", SearchOption.TopDirectoryOnly);
						if (pathsToProfiles.Length == 1)
						{
							FirefoxProfile profile = new FirefoxProfile(pathsToProfiles[0], false);
							profile.AlwaysLoadNoFocusLibrary = true;
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

	public class FiddlerLoginCookieInjector : CommonCookieInjector
	{
		public int ProxyPort { get; set; } = 30000;
		public string Pattern { get; set; }

		protected override Cookies GetCookies(ISpider spider)
		{
			if (string.IsNullOrEmpty(Pattern))
			{
				throw new Exception("Fiddler CookieTrapper: Pattern cannot be null!");
			}

			string cookie;
			using (FiddlerClient fiddlerWrapper = new FiddlerClient(ProxyPort, Pattern))
			{
				fiddlerWrapper.StartCapture(true);
				try
				{
					base.GetCookies(spider);
					var header = fiddlerWrapper.Headers;
					const string cookiesPattern = @"Cookie: (.*?)\r\n";
					cookie = Regex.Match(header, cookiesPattern).Groups[1].Value;
				}
				catch (Exception e)
				{
					LogCenter.Log(null, "Get cookie failed.", Core.Infrastructure.LogLevel.Error, e);
					return null;
				}
				fiddlerWrapper.StopCapture();
			}

			return new Cookies { StringPart = cookie };
		}
	}

	public class CommonCookieInjector : WebDriverCookieInjector
	{
		public string Url { get; set; }

		public string AfterLoginUrl { get; set; }

		public Selector UserSelector { get; set; }

		public string User { get; set; }

		public Selector PassSelector { get; set; }

		public string Pass { get; set; }

		public Selector SubmitSelector { get; set; }

		public Selector LoginAreaSelector { get; set; }

		protected override Cookies GetCookies(ISpider spider)
		{
			var cookies = new Dictionary<string, string>();

			var webDriver = GetWebDriver();
			try
			{
				webDriver.Navigate().GoToUrl(Url);
				Thread.Sleep(10000);

				if (LoginAreaSelector != null)
				{
					try
					{
						var loginArea = FindElement(webDriver, LoginAreaSelector);
						loginArea.Click();
						Thread.Sleep(1000);
					}
					catch (Exception e)
					{
						// ignored
					}
				}

				if (UserSelector != null)
				{
					var user = FindElement(webDriver, UserSelector);
					user.Clear();
					user.SendKeys(User);
					Thread.Sleep(1500);
				}

				if (PassSelector != null)
				{
					var pass = FindElement(webDriver, PassSelector);
					pass.Clear();
					pass.SendKeys(Pass);
					Thread.Sleep(1500);
				}

				var submit = FindElement(webDriver, SubmitSelector);
				submit.Click();
				Thread.Sleep(10000);

				if (!string.IsNullOrEmpty(AfterLoginUrl))
				{
					webDriver.Navigate().GoToUrl(AfterLoginUrl);
					Thread.Sleep(10000);
				}

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
				LogCenter.Log(null, "Get cookie failed.", Core.Infrastructure.LogLevel.Error, e);
				webDriver.Dispose();
				return null;
			}

			return new Cookies { PairPart = cookies };
		}
	}

}
#endif