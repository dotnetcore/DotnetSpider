#if !NET_CORE
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Extension.Downloader.WebDriver;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Extension.Downloader
{
	public class FiddlerDownloader : BaseDownloader
	{
		private readonly int _webDriverWaitTime;
		private readonly Option _option;
		private static bool _isLogined;
		private readonly FiddlerClient _fiddlerClient;
		private IWebDriver _webDriver;
		private static FiddlerDownloader _downloader;

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

			if (_downloader == null)
			{
				_downloader = this;
			}
		}

		public FiddlerDownloader(string urlParten, Option option) : this(urlParten, option, 300)
		{
		}

		public FiddlerDownloader(string urlParten, Option option, Func<RemoteWebDriver, bool> login = null) : this(urlParten, option, 200)
		{
			Login = login;
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
						_webDriver = WebDriverUtil.Open(Browser.Chrome, _option);
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
					Uri uri = request.Url;
					string query = uri.Query;
					string realUrl = uri.Scheme + "://" + uri.DnsSafeHost + ":" + uri.Port + uri.AbsolutePath + (string.IsNullOrEmpty(query) ? "" : ("?" + HttpUtility.UrlPathEncode(uri.Query.Substring(1, uri.Query.Length - 1))));

					if (UrlFormat != null)
					{
						realUrl = UrlFormat(realUrl);
					}

					NetworkCenter.Current.Execute("fid-d", () =>
					{
						_webDriver.Navigate().GoToUrl(realUrl);
					});

					Thread.Sleep(_webDriverWaitTime);

					AfterNavigate?.Invoke((RemoteWebDriver)_webDriver);

					Page page = new Page(request, spider.Site.ContentType, site.RemoveOutboundLinks ? site.Domains : null);
					page.Content = _fiddlerClient.ResponseBodyString;
					_fiddlerClient.Clear();
					page.TargetUrl = _webDriver.Url;
					page.Title = _webDriver.Title;
					// 结束后要置空, 这个值存到Redis会导置无限循环跑单个任务
					request.PutExtra(Request.CycleTriedTimes, null);

					return page;
				}
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
			_fiddlerClient.Dispose();
		}

		public override IDownloader Clone()
		{
			return _downloader;
		}
	}
}

#endif