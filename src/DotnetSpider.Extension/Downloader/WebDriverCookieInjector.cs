using OpenQA.Selenium.Firefox;
using System;
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
using System.Net;
using DotnetSpider.Downloader;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction;
using DotnetSpider.Common;

namespace DotnetSpider.Extension.Downloader
{
	public abstract class WebDriverCookieInjector : CookieInjector
	{
		/// <summary>
		/// 浏览器
		/// </summary>
		private readonly Browser _browser;

		public WebDriverCookieInjector(Browser browser, IControllable controllable) : base(controllable)
		{
			_browser = browser;
		}

		/// <summary>
		/// 创建WebDriver对象
		/// </summary>
		/// <returns>WebDriver对象</returns>
		protected RemoteWebDriver CreateWebDriver()
		{
			RemoteWebDriver webDriver;
			switch (_browser)
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