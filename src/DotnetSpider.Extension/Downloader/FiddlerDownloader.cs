#if !NET_CORE
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Core.Redial;

namespace DotnetSpider.Extension.Downloader
{
	public class FiddlerDownloader : BaseDownloader
	{
		private readonly object _locker = new object();
		private readonly int _webDriverWaitTime;
		private readonly Option _option;
		private static bool _isLogined;
		private readonly FiddlerClient _fiddlerClient;
		private IWebDriver _webDriver;

		public Func<RemoteWebDriver, bool> Login;
		public Func<string, string> UrlFormat;
		public Func<RemoteWebDriver, bool> AfterNavigate;

		public FiddlerDownloader(string urlParten, Option option, int webDriverWaitTime = 200)
		{
			_option = option;
			_option.Proxy = "127.0.0.1:30000";
			_webDriverWaitTime = webDriverWaitTime;

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

			_fiddlerClient = new FiddlerClient(30000, urlParten);
			_fiddlerClient.StartCapture();
		}

		public FiddlerDownloader(string urlParten, Option option) : this(urlParten, option, 300)
		{
		}

		public FiddlerDownloader(string urlParten, Option option, Func<RemoteWebDriver, bool> login = null) : this(urlParten, option, 200)
		{
			Login = login;
		}

		public override void Dispose()
		{
			_fiddlerClient.Dispose();
		}

		protected override Page DowloadContent(Request request, ISpider spider)
		{
			Site site = spider.Site;
			try
			{
				lock (_locker)
				{
					if (_webDriver == null)
					{
						_webDriver = WebDriverExtensions.Open(Browser.Chrome, _option);
					}
					if (!_isLogined && Login != null)
					{
						_isLogined = Login.Invoke(_webDriver as RemoteWebDriver);
						if (!_isLogined)
						{
							throw new SpiderException("Login failed. Please check your login codes.");
						}
					}

					//中文乱码URL
					string query = request.Uri.Query;
					string realUrl = request.Uri.Scheme + "://" + request.Uri.DnsSafeHost + ":" + request.Uri.Port + request.Uri.AbsolutePath + (string.IsNullOrEmpty(query) ? "" : ("?" + HttpUtility.UrlPathEncode(request.Uri.Query.Substring(1, request.Uri.Query.Length - 1))));

					if (UrlFormat != null)
					{
						realUrl = UrlFormat(realUrl);
					}

					NetworkCenter.Current.Execute("fiddler-download", () =>
					{
						_webDriver.Navigate().GoToUrl(realUrl);
					});

					Thread.Sleep(_webDriverWaitTime);

					AfterNavigate?.Invoke((RemoteWebDriver)_webDriver);

					Page page = new Page(request);
					page.Content = _fiddlerClient.ResponseBodyString;
					_fiddlerClient.Clear();
					page.TargetUrl = _webDriver.Url;
					// 结束后要置空, 这个值存到Redis会导置无限循环跑单个任务
					//request.PutExtra(Request.CycleTriedTimes, null);

					return page;
				}
			}
			catch (DownloadException)
			{
				throw;
			}
			catch (Exception e)
			{
				Page page = new Page(request) { Exception = e };
				return page;
			}
		}
	}
}

#endif