using System;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Infrastructure;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using DotnetSpider.Core.Redial;
using System.Runtime.InteropServices;
using DotnetSpider.Extension.Infrastructure;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Extension.Downloader
{
	/// <summary>
	/// 使用 WebDriver 作为下载器
	/// </summary>
	public class WebDriverDownloader : BaseDownloader
	{
		private readonly object _locker = new object();
		private IWebDriver _webDriver;
		private readonly int _webDriverWaitTime;
		private bool _isLogined;
		private readonly Browser _browser;
		private readonly Option _option;
		private bool _isDisposed;
		private string[] _domains;

		public event Action<RemoteWebDriver> NavigateCompeleted;

		public WebDriverDownloader(Browser browser, string[] domains = null, int webDriverWaitTime = 200, Option option = null)
		{
			_webDriverWaitTime = webDriverWaitTime;
			_browser = browser;
			_option = option ?? new Option();
			_domains = domains == null ? new string[0] : domains;

			if (browser == Browser.Firefox && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				Task.Factory.StartNew(() =>
				{
					while (!_isDisposed)
					{
						IntPtr maindHwnd = WindowsFormUtil.FindWindow(null, "plugin-container.exe - 应用程序错误");
						if (maindHwnd != IntPtr.Zero)
						{
							WindowsFormUtil.SendMessage(maindHwnd, WindowsFormUtil.WmClose, 0, 0);
						}
						Thread.Sleep(500);
					}
				});
			}
		}

		public WebDriverDownloader(Browser browser, Option option) : this(browser, null, 200, option) { }

		public override void Dispose()
		{
			_isDisposed = true;
			_webDriver?.Quit();
		}

		protected override Page DowloadContent(Request request, ISpider spider)
		{
			Site site = spider.Site;
			try
			{
				lock (_locker)
				{
					_webDriver = _webDriver ?? WebDriverExtensions.Open(_browser, _option);

					foreach (var domain in _domains)
					{
						var cookies = _cookieContainer.GetCookies(new Uri(domain));
						foreach (System.Net.Cookie cookie in cookies)
						{
							AddCookieToDownloadClient(cookie);
						}
					}

					if (!_isLogined && CookieInjector != null)
					{
						var webdriverLoginHandler = CookieInjector as WebDriverLoginHandler;
						if (webdriverLoginHandler != null)
						{
							webdriverLoginHandler.Driver = _webDriver as RemoteWebDriver;
						}
						CookieInjector.Inject(this, spider);
						_isLogined = true;
					}
				}

				//#if NET_CORE
				//				string query = string.IsNullOrEmpty(uri.Query) ? "" : $"?{WebUtility.UrlEncode(uri.Query.Substring(1, uri.Query.Length - 1))}";
				//#else
				//				string query = string.IsNullOrEmpty(uri.Query) ? "" : $"?{HttpUtility.UrlPathEncode(uri.Query.Substring(1, uri.Query.Length - 1))}";
				//#endif
				//				string realUrl = $"{uri.Scheme}://{uri.DnsSafeHost}{(uri.Port == 80 ? "" : ":" + uri.Port)}{uri.AbsolutePath}{query}";

				var domainUrl = $"{request.Uri.Scheme}://{request.Uri.DnsSafeHost}{(request.Uri.Port == 80 ? "" : ":" + request.Uri.Port)}";

				// TODO:重新实现WebDriverDownloader设置Cookie
				//var options = _webDriver.Manage();
				//if (options.Cookies.AllCookies.Count == 0 && spider.Site.Cookies?.PairPart.Count > 0)
				//{
				//	_webDriver.Url = domainUrl;
				//	options.Cookies.DeleteAllCookies();
				//	if (spider.Site.Cookies != null)
				//	{
				//		foreach (var c in spider.Site.Cookies.PairPart)
				//		{
				//			options.Cookies.AddCookie(new Cookie(c.Key, c.Value));
				//		}
				//	}
				//}

				string realUrl = request.Url.ToString();

				NetworkCenter.Current.Execute("webdriver-download", () =>
				{
					_webDriver.Navigate().GoToUrl(realUrl);

					NavigateCompeleted?.Invoke((RemoteWebDriver)_webDriver);
				});

				Thread.Sleep(_webDriverWaitTime);

				Page page = new Page(request)
				{
					Content = _webDriver.PageSource,
					TargetUrl = _webDriver.Url
				};

				// 结束后要置空, 这个值存到Redis会导置无限循环跑单个任务
				//request.PutExtra(Request.CycleTriedTimes, null);
				return page;
			}
			catch (DownloadException de)
			{
				Page page = new Page(request) { Exception = de };
				if (site.CycleRetryTimes > 0)
				{
					page = Spider.AddToCycleRetry(request, site);
				}
				Logger.Log(spider.Identity, $"下载 {request.Url} 失败: {de.Message}.", Level.Warn);
				return page;
			}
			catch (Exception e)
			{
				Logger.Log(spider.Identity, $"下载 {request.Url} 失败: {e.Message}.", Level.Warn);
				Page page = new Page(request) { Exception = e };
				return page;
			}
		}

		/// <summary>
		/// 设置 Cookie
		/// </summary>
		/// <param name="cookie">Cookie</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		protected override void AddCookieToDownloadClient(System.Net.Cookie cookie)
		{
			_webDriver?.Manage().Cookies.AddCookie(new Cookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path, null));
		}
	}
}
