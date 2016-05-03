#if !NET_CORE
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Downloader;
using Java2Dotnet.Spider.Extension.Downloader.WebDriver;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Redial;
using OpenQA.Selenium.Remote;

namespace Java2Dotnet.Spider.Extension.Downloader
{
	public class FiddlerDownloader : BaseDownloader
	{
		private volatile WebDriverPool _webDriverPool;
		private readonly int _webDriverWaitTime;
		private readonly Option _option;
		private static bool _isLogined;
		private readonly FiddlerClient _fiddlerClient;

		public Func<RemoteWebDriver, bool> Login;
		public Func<string, string> UrlFormat;
		public Func<RemoteWebDriver, bool> AfterNavigate;

		public FiddlerDownloader(string urlParten, int webDriverWaitTime = 200, Option option = null)
		{
			// Fiddler只能单线程, 否则可能会造成数据不对应.
			ThreadNum = 1;
			_option = option ?? new Option();
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

		public FiddlerDownloader(string urlParten) : this(urlParten, 300)
		{
		}

		public FiddlerDownloader(string urlParten,
			Func<RemoteWebDriver, bool> login = null) : this(urlParten, 200, null)
		{
			Login = login;
		}

		public override Page Download(Request request, ISpider spider)
		{
			WebDriverItem driverService = null;

			try
			{
				driverService = Pool.Get();

				lock (this)
				{
					if (!_isLogined && Login != null)
					{
						_isLogined = Login.Invoke(driverService.WebDriver as RemoteWebDriver);
						if (!_isLogined)
						{
							throw new SpiderExceptoin("Login failed. Please check your login codes.");
						}
					}
				}

				//中文乱码URL
				Uri uri = request.Url;
				string query = uri.Query;
				string realUrl = uri.Scheme + "://" + uri.DnsSafeHost + ":" + uri.Port + uri.AbsolutePath + (string.IsNullOrEmpty(query) ? "" : ("?" + HttpUtility.UrlPathEncode(uri.Query.Substring(1, uri.Query.Length - 1))));

				if (UrlFormat != null)
				{
					realUrl = UrlFormat(realUrl);
				}

				RedialManagerUtils.Execute("webdriverdownloader-download", () =>
				{
					driverService.WebDriver.Navigate().GoToUrl(realUrl);
				});

				Thread.Sleep(_webDriverWaitTime);

				AfterNavigate?.Invoke((RemoteWebDriver)driverService.WebDriver);

				Page page = new Page(request, spider.Site.ContentType);
				page.Content = _fiddlerClient.ResponseBodyString;
				_fiddlerClient.Clear();
				page.Url = request.Url.ToString();
				page.TargetUrl = driverService.WebDriver.Url;
				page.Title = driverService.WebDriver.Title;

				ValidatePage(page, spider);

				// 结束后要置空, 这个值存到Redis会导置无限循环跑单个任务
				request.PutExtra(Request.CycleTriedTimes, null);

				return page;
			}
			finally
			{
				Pool.ReturnToPool(driverService);
			}
		}

		public override void Dispose()
		{
			_fiddlerClient.Dispose();
			Pool?.CloseAll();
		}

		private WebDriverPool Pool
		{
			get
			{
				if (_webDriverPool == null)
				{
					_webDriverPool = new WebDriverPool(Browser.Firefox, ThreadNum, _option);
				}
				return _webDriverPool;
			}
		}
	}
}

#endif