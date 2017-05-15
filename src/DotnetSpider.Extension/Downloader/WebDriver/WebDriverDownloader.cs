#if !NET_CORE
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Infrastructure;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;

namespace DotnetSpider.Extension.Downloader.WebDriver
{
	public class WebDriverDownloader : BaseDownloader
	{
		private IWebDriver _webDriver;
		private readonly int _webDriverWaitTime;
		private static bool _isLogined;
		private readonly Browser _browser;
		private readonly Option _option;
		public IWebDriverHandler SignIn { get; set; }
		public IWebDriverHandler VerifyCode { get; set; }
		public Func<string, string> UrlHandler;
		public IWebDriverHandler NavigateCompeleted;

		public WebDriverDownloader(Browser browser, int webDriverWaitTime = 200, Option option = null)
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

		public WebDriverDownloader(Browser browser) : this(browser, 300)
		{
		}

		public WebDriverDownloader(Browser browser, IWebDriverHandler siginIn) : this(browser, 200, null)
		{
			SignIn = siginIn;
		}

		protected override Page DowloadContent(Request request, ISpider spider)
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

					if (!_isLogined && SignIn != null)
					{
						_isLogined = SignIn.Handle(_webDriver as RemoteWebDriver);
						if (!_isLogined)
						{
							throw new SpiderException("Login failed. Please check your login codes.");
						}
					}
				}

				//中文乱码URL
				Uri uri = request.Url;
				string query = string.IsNullOrEmpty(uri.Query) ? "" : $"?{HttpUtility.UrlPathEncode(uri.Query.Substring(1, uri.Query.Length - 1))}";
				string realUrl = $"{uri.Scheme}://{uri.DnsSafeHost}{(uri.Port == 80 ? "" : ":" + uri.Port)}{uri.AbsolutePath}{query}";

				var domainUrl = $"{uri.Scheme}://{uri.DnsSafeHost}{(uri.Port == 80 ? "" : ":" + uri.Port)}";
				var options = _webDriver.Manage();
				if (options.Cookies.AllCookies.Count == 0 && spider.Site.Cookies.PairPart.Count > 0)
				{
					_webDriver.Url = domainUrl;
					options.Cookies.DeleteAllCookies();
					foreach (var c in spider.Site.Cookies.PairPart)
					{
						options.Cookies.AddCookie(new Cookie(c.Key, c.Value));
					}
				}

				if (UrlHandler != null)
				{
					realUrl = UrlHandler(realUrl);
				}

				NetworkCenter.Current.Execute("wd-d", () =>
				{
					_webDriver.Navigate().GoToUrl(realUrl);

					NavigateCompeleted?.Handle((RemoteWebDriver)_webDriver);
				});

				Thread.Sleep(_webDriverWaitTime);

				Page page = new Page(request, spider.Site.ContentType, site.RemoveOutboundLinks ? site.Domains : null)
				{
					Content = _webDriver.PageSource,
					TargetUrl = _webDriver.Url,
					Title = _webDriver.Title
				};

				// 结束后要置空, 这个值存到Redis会导置无限循环跑单个任务
				request.PutExtra(Request.CycleTriedTimes, null);
				return page;
			}
			catch (DownloadException)
			{
				throw;
			}
			catch (Exception e)
			{
				Page page = new Page(request, site.ContentType, null) { Exception = e };
				return page;
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