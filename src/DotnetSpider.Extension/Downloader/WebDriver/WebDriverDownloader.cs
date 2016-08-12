#if !NET_CORE
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Common;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DotnetSpider.Extension.Downloader.WebDriver
{
	public class WebDriverDownloader : BaseDownloader
	{
		private IWebDriver _webDriver;
		private readonly int _webDriverWaitTime;
		private static bool _isLogined;
		private readonly Browser _browser;
		private readonly Option _option;
		public Func<RemoteWebDriver, bool> Login { get; set; }
		public Func<RemoteWebDriver, bool> VerifyCode { get; set; }
		public Func<string, string> UrlFormat;
		public Func<RemoteWebDriver, bool> AfterNavigate;

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

		public WebDriverDownloader(Browser browser, Func<RemoteWebDriver, bool> login) : this(browser, 200, null)
		{
			Login = login;
		}

		public override Page Download(Request request, ISpider spider)
		{
			Site site = spider.Site;
			BeforeDownload(request, spider);
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
                var domainUrl = string.Format("{0}://{1}{2}", uri.Scheme, uri.DnsSafeHost, uri.Port == 80 ? "" : (":" + uri.Port));
                //       string realUrl = domainUrl + uri.AbsolutePath + (string.IsNullOrEmpty(query)
                //? ""
                //: ("?" + HttpUtility.UrlPathEncode(uri.Query.Substring(1, uri.Query.Length - 1))));
                string realUrl = HttpUtility.UrlPathEncode(request.Url.AbsoluteUri);

                var options = _webDriver.Manage();
                if (options.Cookies.AllCookies.Count == 0 && spider.Site.Cookies.Count > 0)
                {
                    _webDriver.Url = domainUrl;
                    options.Cookies.DeleteAllCookies();
                    foreach (var c in spider.Site.Cookies)
                    {
                        options.Cookies.AddCookie(c);
                    }
                }

                if (UrlFormat != null)
				{
					realUrl = UrlFormat(realUrl);
				}

				NetworkCenter.Current.Execute("wd-d", () =>
				{
					_webDriver.Navigate().GoToUrl(realUrl);
				});

                if (Context != null)
                {
                    var context = Context as EntitySpider;
                    var untilMethod = context?.GetConditionMethodByUrl(realUrl);
                    if (untilMethod != null)
                    {
                        dynamic condition;
                        WebDriverWait wait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(10));
                        condition = untilMethod.Invoke(null, null);
                        wait.Until(condition);
                    }
                }

                Thread.Sleep(_webDriverWaitTime);

				AfterNavigate?.Invoke((RemoteWebDriver)_webDriver);

				Page page = new Page(request, spider.Site.ContentType);
				page.Content = _webDriver.PageSource;
				page.Url = request.Url.ToString();
				page.TargetUrl = _webDriver.Url;
				page.Title = _webDriver.Title;

				// 结束后要置空, 这个值存到Redis会导置无限循环跑单个任务
				request.PutExtra(Request.CycleTriedTimes, null);

				AfterDownloadComplete(page, spider);

				return page;
			}
			catch (DownloadException)
			{
				throw;
			}
			catch (Exception e)
			{
				Page page = new Page(request, site.ContentType) { Exception = e };

				AfterDownloadComplete(page, spider);
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