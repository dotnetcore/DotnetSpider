#if !NET_CORE
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.JLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.PhantomJS;

namespace Java2Dotnet.Spider.Extension.Downloader.WebDriver
{
	public class WebDriverPool
	{
		//private readonly ILog _logger = LogManager.GetLogger(typeof(WebDriverPool));
		protected static readonly ILog _logger = LogManager.GetLogger();
		private static int DEFAULT_CAPACITY = 5;

		private readonly int _capacity;

		private readonly Option _option;

		private static int STAT_RUNNING = 1;

		private static int STAT_CLODED = 2;

		private readonly AtomicInteger _stat = new AtomicInteger(STAT_RUNNING);

		private readonly Browser _browser;

		private readonly BlockingCollection<WebDriverItem> _webDriverList = new BlockingCollection<WebDriverItem>();
		private readonly ConcurrentQueue<WebDriverItem> _innerQueue = new ConcurrentQueue<WebDriverItem>();

		public WebDriverPool(Browser browser, int capacity = 5, Option option = null)
		{
			_capacity = capacity;
			_browser = browser;
			_option = option ?? new Option();
		}

		public WebDriverPool() : this(Browser.Phantomjs, DEFAULT_CAPACITY)
		{
		}

		public WebDriverItem Get()
		{
			CheckRunning();

			if (_webDriverList.Count < _capacity)
			{
				if (_innerQueue.Count == 0)
				{
					IWebDriver e = null;
					switch (_browser)
					{
						case Browser.Phantomjs:
							var phantomJsDriverService = PhantomJSDriverService.CreateDefaultService();
							if (!string.IsNullOrEmpty(_option.Proxy))
							{
								phantomJsDriverService.Proxy = _option.Proxy;
								phantomJsDriverService.ProxyAuthentication = _option.ProxyAuthentication;
							}
							e = new PhantomJSDriver(phantomJsDriverService);
							break;
						case Browser.Firefox:
							string path = Environment.ExpandEnvironmentVariables("%APPDATA%") + @"\Mozilla\Firefox\Profiles\";
							string[] pathsToProfiles = Directory.GetDirectories(path, "*.webdriver", SearchOption.TopDirectoryOnly);
							FirefoxProfile profile;
							if (pathsToProfiles.Length == 1)
							{
								profile = new FirefoxProfile(pathsToProfiles[0], false);
							}
							else
							{
								profile = new FirefoxProfile();
							}
							if (!_option.AlwaysLoadNoFocusLibrary)
							{
								profile.AlwaysLoadNoFocusLibrary = true;
							}

							if (!_option.LoadImage)
							{
								profile.SetPreference("permissions.default.image", 2);
							}
							if (!_option.LoadFlashPlayer)
							{
								profile.SetPreference("dom.ipc.plugins.enabled.libflashplayer.so", "false");
							}
							if (!string.IsNullOrEmpty(_option.Proxy))
							{
								string[] p = _option.Proxy.Split(':');
								string host = p[0];
								int port = Convert.ToInt32(p[1]);
								profile.SetPreference("network.proxy.ftp_port", port);
								profile.SetPreference("network.proxy.gopher", host);
								profile.SetPreference("network.proxy.gopher_port", port);
								profile.SetPreference("network.proxy.http", host);
								profile.SetPreference("network.proxy.http_port", port);
								profile.SetPreference("network.proxy.no_proxies_on", "localhost,127.0.0.1,<-loopback>");
								profile.SetPreference("network.proxy.share_proxy_settings", true);
								profile.SetPreference("network.proxy.socks", host);
								profile.SetPreference("network.proxy.socks_port", port);
								profile.SetPreference("network.proxy.ssl", host);
								profile.SetPreference("network.proxy.ssl_port", port);
								profile.SetPreference("network.proxy.type", 1);
							}

							e = new FirefoxDriver(profile);
							break;
						case Browser.Chrome:
							ChromeDriverService cds = ChromeDriverService.CreateDefaultService();
							cds.HideCommandPromptWindow = true;
							ChromeOptions opt = new ChromeOptions();
							if (!_option.LoadImage)
							{
								opt.AddUserProfilePreference("profile", new { default_content_setting_values = new { images = 2 } });
							}
							if (!string.IsNullOrEmpty(_option.Proxy))
							{
								opt.Proxy = new Proxy() { HttpProxy = _option.Proxy };
							}
							e = new ChromeDriver(cds, opt);
							break;
					}
					_innerQueue.Enqueue(new WebDriverItem(e));
					_webDriverList.Add(new WebDriverItem(e));
				}
			}

			//else
			//{
			//	while (true)
			//	{
			//		lock (_innerQueue)
			//		{
			//			if (_innerQueue.Count > 0)
			//			{
			//				break;
			//			}
			//			Thread.Sleep(150);
			//		}
			//	}
			//}

			WebDriverItem webDriver;
			while (!_innerQueue.TryDequeue(out webDriver))
			{
				Thread.Sleep(150);
			}

			return webDriver;
		}

		public void ReturnToPool(WebDriverItem webDriver)
		{
			CheckRunning();

			if (_webDriverList.Contains(webDriver))
			{
				_innerQueue.Enqueue(webDriver);
			}
		}

		private void CheckRunning()
		{
			if (!_stat.CompareAndSet(STAT_RUNNING, STAT_RUNNING))
			{
				throw new SpiderExceptoin("Already closed!");
			}
		}

		public void CloseAll()
		{
			lock (_innerQueue)
			{
				bool b = _stat.CompareAndSet(STAT_RUNNING, STAT_CLODED);
				if (!b)
				{
					throw new SpiderExceptoin("Already closed!");
				}

				foreach (WebDriverItem webDriver in _webDriverList)
				{
					_logger.Info("Quit webDriver" + webDriver);
					Close(webDriver);
				}
			}
		}

		public void Close(WebDriverItem webDriver)
		{
			try
			{
				if (_webDriverList.Contains(webDriver))
				{
					_webDriverList.TryTake(out webDriver);
					webDriver.WebDriver.Quit();
				}
			}
			catch (Exception)
			{
				// ignored
			}
		}
	}
}
#endif