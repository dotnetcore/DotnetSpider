using System;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Core.Infrastructure;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using System.Runtime.InteropServices;
using DotnetSpider.Extension.Infrastructure;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using DotnetSpider.Downloader;
using DotnetSpider.Common;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Collections;

namespace DotnetSpider.Extension.Downloader
{
	/// <summary>
	/// 使用 WebDriver 作为下载器
	/// </summary>
	public class WebDriverDownloader : DotnetSpider.Downloader.Downloader, IBeforeDownloadHandler
	{
		private IWebDriver _driver;
		private readonly int _driverWaitTime;
		private readonly Browser _browser;
		private readonly Option _option;
		private bool _isDisposed;
		private readonly IEnumerable<string> _domains;

		/// <summary>
		/// 每次navigate完成后, WebDriver 需要执行的操作
		/// </summary>
		public List<IWebDriverAction> Actions;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="browser">浏览器</param>
		/// <param name="domains">被采集链接的Domain, Cookie</param>
		/// <param name="webDriverWaitTime">请求链接完成后需要等待的时间</param>
		/// <param name="option">选项</param>
		public WebDriverDownloader(Browser browser, IEnumerable<string> domains = null, int webDriverWaitTime = 200,
			Option option = null)
		{
			_driverWaitTime = webDriverWaitTime;
			_browser = browser;
			_option = option ?? new Option();
			_domains = domains;
			AddBeforeDownloadHandler(this);
			AutoCloseErrorDialog();
		}

		private void AutoCloseErrorDialog()
		{
#if NETFRAMEWORK
			bool requireCloseErrorDialog = _browser == Browser.Firefox;
#else
			bool requireCloseErrorDialog = _browser == Browser.Firefox && RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
			if (requireCloseErrorDialog)
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
			_driver?.Quit();
		}

		public override void AddCookie(System.Net.Cookie cookie)
		{
			base.AddCookie(cookie);
			// 如果 Downloader 在运行中, 需要把 Cookie 加到 Driver 中
			_driver?.Manage().Cookies.AddCookie(new OpenQA.Selenium.Cookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path, null));
		}

		public void Handle(ref Request request, IDownloader downloader)
		{
			var d = downloader as WebDriverDownloader;
			if (d != null && d._driver == null)
			{
				d._driver = WebDriverUtil.Open(_browser, _option);
				d._driver.Url = request.Url;
				Logger?.Information("实例化浏览器");

				foreach (var domain in _domains)
				{
					var cookies = GetAllCookies(CookieContainer);
					foreach (System.Net.Cookie cookie in cookies)
					{
						// 此处不能通过直接调用AddCookie来添加, 会导致CookieContainer添加重复值
						d._driver.Manage().Cookies.AddCookie(new OpenQA.Selenium.Cookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path, null));
					}
				}
			}
		}

		private List<System.Net.Cookie> GetAllCookies(CookieContainer cc)
		{
			var lstCookies = new List<System.Net.Cookie>();

			Hashtable table = (Hashtable)cc.GetType().InvokeMember("m_domainTable",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField |
				System.Reflection.BindingFlags.Instance, null, cc, new object[] { });

			foreach (object pathList in table.Values)
			{
				SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list",
					System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField
					| System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
				foreach (CookieCollection colCookies in lstCookieCol.Values)
					foreach (System.Net.Cookie c in colCookies) lstCookies.Add(c);
			}

			return lstCookies;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		protected override Common.Response DowloadContent(Request request)
		{
			try
			{
				NetworkCenter.Current.Execute("webdriver-download", () =>
				{
					_driver.Navigate().GoToUrl(request.Url);

					if (Actions != null)
					{
						foreach (var action in Actions)
						{
							action.Invoke((RemoteWebDriver)_driver);
						}
					}
				});

				Thread.Sleep(_driverWaitTime);

				var response = new Common.Response(request) { Content = _driver.PageSource };
				DetectContentType(response, null);
				return response;
			}
			catch (DownloaderException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new DownloaderException($"Unexpected exception when download request: {request.Url}: {e}.");
			}
		}
	}
}