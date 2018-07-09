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
using System.Collections.Generic;

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
		private readonly List<string> _domains = new List<string>();

		/// <summary>
		/// 每次navigate完成后, WebDriver 需要执行的操作
		/// </summary>
		public List<IWebDriverHandler> WebDriverHandlers;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="browser">浏览器</param>
		/// <param name="domains">被采集链接的Domain, Cookie</param>
		/// <param name="webDriverWaitTime">请求链接完成后需要等待的时间</param>
		/// <param name="option">选项</param>
		public WebDriverDownloader(Browser browser, string[] domains = null, int webDriverWaitTime = 200,
			Option option = null)
		{
			_webDriverWaitTime = webDriverWaitTime;
			_browser = browser;
			_option = option ?? new Option();
			if (domains != null)
			{
				foreach (var domain in domains)
				{
					if (!string.IsNullOrWhiteSpace(domain) && !_domains.Contains(domain))
					{
						_domains.Add(domain);
					}
				}
			}

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

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="browser">浏览器</param>
		/// <param name="option">选项</param>
		public WebDriverDownloader(Browser browser, Option option) : this(browser, null, 200, option)
		{
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			_isDisposed = true;
			_webDriver?.Quit();
		}

		/// <summary>
		/// 下载工作的具体实现
		/// </summary>
		/// <param name="request">请求信息</param>
		/// <param name="spider">爬虫</param>
		/// <returns>页面数据</returns>
		protected override Task<Page> DowloadContent(Request request, ISpider spider)
		{
			Site site = spider.Site;
			try
			{
				lock (_locker)
				{
					if (_webDriver == null)
					{
						_webDriver = WebDriverUtil.Open(_browser, _option);

						if (_domains != null)
						{
							foreach (var domain in _domains)
							{
								var cookies = CookieContainer.GetCookies(new Uri(domain));
								foreach (System.Net.Cookie cookie in cookies)
								{
									AddCookieToDownloadClient(cookie);
								}
							}
						}

						if (!_isLogined && CookieInjector != null)
						{
							if (CookieInjector is WebDriverLoginHandler webdriverLoginHandler)
							{
								webdriverLoginHandler.Driver = _webDriver as RemoteWebDriver;
							}

							CookieInjector.Inject(this, spider);
							_isLogined = true;
						}
					}
				}

				//#if NET_CORE
				//				string query = string.IsNullOrEmpty(uri.Query) ? "" : $"?{WebUtility.UrlEncode(uri.Query.Substring(1, uri.Query.Length - 1))}";
				//#else
				//				string query = string.IsNullOrEmpty(uri.Query) ? "" : $"?{HttpUtility.UrlPathEncode(uri.Query.Substring(1, uri.Query.Length - 1))}";
				//#endif
				//				string realUrl = $"{uri.Scheme}://{uri.DnsSafeHost}{(uri.Port == 80 ? "" : ":" + uri.Port)}{uri.AbsolutePath}{query}";

//				var domainUrl =
//					$"{request.Uri.Scheme}://{request.Uri.DnsSafeHost}{(request.Uri.Port == 80 ? "" : ":" + request.Uri.Port)}";

				string realUrl = request.Url ;

				NetworkCenter.Current.Execute("webdriver-download", () =>
				{
					_webDriver.Navigate().GoToUrl(realUrl);

					if (WebDriverHandlers != null)
					{
						foreach (var handler in WebDriverHandlers)
						{
							handler.Handle((RemoteWebDriver) _webDriver);
						}
					}
				});

				Thread.Sleep(_webDriverWaitTime);

				Page page = new Page(request)
				{
					Content = _webDriver.PageSource,
					TargetUrl = _webDriver.Url
				};

				return Task.FromResult(page);
			}
			catch (DownloadException de)
			{
				Page page = new Page(request) {Exception = de};
				if (site.CycleRetryTimes > 0)
				{
					page = site.AddToCycleRetry(request);
				}

				spider.Logger.Error($"下载 {request.Url} 失败: {de.Message}.");
				return Task.FromResult(page);
			}
			catch (Exception e)
			{
				spider.Logger.Error($"下载 {request.Url} 失败: {e.Message}.");
				Page page = new Page(request) {Exception = e};
				return Task.FromResult(page);
			}
		}

		/// <summary>
		/// 设置 Cookie
		/// </summary>
		/// <param name="cookie">Cookie</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		protected override void AddCookieToDownloadClient(System.Net.Cookie cookie)
		{
			if (!_domains.Contains(cookie.Domain))
			{
				_domains.Add(cookie.Domain);
			}

			_webDriver?.Manage().Cookies.AddCookie(new Cookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path, null));
		}
	}
}