#if !NET_CORE
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Downloader;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Redial;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;

namespace Java2Dotnet.Spider.Extension.Downloader.WebDriver
{
	public class WebDriverDownloader : BaseDownloader
	{
		private IWebDriver _webDriver;
		private readonly int _webDriverWaitTime;
		private static bool _isLogined;
		private Browser _browser;
		private Option _option;
		public Func<RemoteWebDriver, bool> Login { get; set; }
		public Func<RemoteWebDriver, bool> VerifyCode { get; set; }
		public Func<string, string> UrlFormat;
		public Func<RemoteWebDriver, bool> AfterNavigate;

		public WebDriverDownloader(Browser browser = Browser.Chrome, int webDriverWaitTime = 200, Option option = null)
		{
			_webDriverWaitTime = webDriverWaitTime;
			_browser = browser;
			_option = option ?? new Option();

			if (browser == Browser.Firefox)
			{
				Task.Factory.StartNew(() =>
				{
					while (true)
					{
						IntPtr maindHwnd = WindowsFormUtil.FindWindow(null, "plugin-container.exe - 应用程序错误");
						if (maindHwnd != IntPtr.Zero)
						{
							WindowsFormUtil.SendMessage(maindHwnd, WindowsFormUtil.WmClose, 0, 0);
						}
						Thread.Sleep(500);
					}
					// ReSharper disable once FunctionNeverReturns
				});
			}
		}

		public WebDriverDownloader(Browser browser = Browser.Phantomjs) : this(browser, 300)
		{
		}

		public WebDriverDownloader(Browser browser = Browser.Phantomjs,
			Func<RemoteWebDriver, bool> login = null) : this(browser, 200, null)
		{
			Login = login;
		}

		public override Page Download(Request request, ISpider spider)
		{
			Site site = spider.Site;
			try
			{
				lock (this)
				{
					if (_webDriver == null)
					{
						_webDriver = WebDriverUtil.Open(_browser, _option);
					}

					if (!_isLogined && Login != null)
					{
						_isLogined = Login.Invoke(_webDriver as RemoteWebDriver);
						if (!_isLogined)
						{
							throw new SpiderException("Login failed. Please check your login codes.");
						}
					}
				}

				//中文乱码URL
				Uri uri = request.Url;
				string query = uri.Query;
				string realUrl = uri.Scheme + "://" + uri.DnsSafeHost + (uri.Port == 80 ? "" : (":" + uri.Port)) + uri.AbsolutePath + (string.IsNullOrEmpty(query)
									? ""
									: ("?" + HttpUtility.UrlPathEncode(uri.Query.Substring(1, uri.Query.Length - 1))));

				if (UrlFormat != null)
				{
					realUrl = UrlFormat(realUrl);
				}

				NetworkProxyManager.Current.Execute("wd-d", () =>
				{
					_webDriver.Navigate().GoToUrl(realUrl);
				});

				Thread.Sleep(_webDriverWaitTime);

				AfterNavigate?.Invoke((RemoteWebDriver)_webDriver);

				Page page = new Page(request, spider.Site.ContentType);
				page.Content = _webDriver.PageSource;
				page.Url = request.Url.ToString();
				page.TargetUrl = _webDriver.Url;
				page.Title = _webDriver.Title;

				// 结束后要置空, 这个值存到Redis会导置无限循环跑单个任务
				request.PutExtra(Request.CycleTriedTimes, null);

				Handle(page, spider);

				return page;
			}
			catch (DownloadException)
			{
				throw;
			}
			catch (Exception e)
			{
				Page page = new Page(request, site.ContentType) { Exception = e };

				Handle(page, spider);
				throw;
			}
		}

		public override void Dispose()
		{
			_webDriver.Quit();
			_webDriver.Close();
		}
	}
}
#endif