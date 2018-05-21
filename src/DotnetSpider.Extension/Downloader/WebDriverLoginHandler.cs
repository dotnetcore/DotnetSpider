using DotnetSpider.Core;
using System;
using DotnetSpider.Core.Selector;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System.Threading;
using DotnetSpider.Extension.Model;
using DotnetSpider.Core.Downloader;
using System.Net;
using Serilog;

namespace DotnetSpider.Extension.Downloader
{
	/// <summary>
	/// 通过WebDriver的登录操作的抽象
	/// </summary>
	public abstract class WebDriverLoginHandler : LoginHandler
	{
		/// <summary>
		/// WebDriver
		/// </summary>
		public RemoteWebDriver Driver { get; set; }

		/// <summary>
		/// 构造方法
		/// </summary>
		public WebDriverLoginHandler()
		{
		}

		/// <summary>
		/// 取得所有Cookie
		/// </summary>
		/// <param name="spider">爬虫</param>
		/// <returns>Cookie</returns>
		protected override CookieCollection GetCookies(ISpider spider)
		{
			return new CookieCollection();
		}

		/// <summary>
		/// 执行注入Cookie的操作
		/// </summary>
		/// <param name="downloader">下载器</param>
		/// <param name="spider">需要注入Cookie的爬虫</param>
		/// <param name="pauseBeforeInject">注入Cookie前是否先暂停爬虫</param>
		public override void Inject(IDownloader downloader, ISpider spider, bool pauseBeforeInject = true)
		{
			if (Driver == null)
			{
				return;
			}
			if (!CheckFrequency())
			{
				return;
			}
			spider.Pause(() =>
			{
				Login();
				spider.Contiune();
			});
		}

		/// <summary>
		/// 登录操作的实现
		/// </summary>
		/// <returns>是否登录成功</returns>
		protected abstract bool Login();
	}

	/// <summary>
	/// 通用的登录操作
	/// </summary>
	public class CommonLoginHandler : WebDriverLoginHandler
	{
		/// <summary>
		/// 登陆的链接
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// 用户名在网页中的元素选择器
		/// </summary>
		public SelectorAttribute UserSelector { get; set; }

		/// <summary>
		/// 用户名
		/// </summary>
		public string User { get; set; }

		/// <summary>
		/// 密码在网页中的元素选择器
		/// </summary>
		public SelectorAttribute PasswordSelector { get; set; }

		/// <summary>
		/// 密码
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// 登陆按钮的元素选择器
		/// </summary>
		public SelectorAttribute SubmitSelector { get; set; }

		/// <summary>
		/// 登录操作的实现
		/// </summary>
		/// <returns>是否登录成功</returns>
		protected override bool Login()
		{
			try
			{
				Driver.Navigate().GoToUrl(Url);
				Thread.Sleep(5000);

				var user = FindElement(Driver, UserSelector);
				user.Clear();
				user.SendKeys(User);
				Thread.Sleep(1000);

				var password = FindElement(Driver, PasswordSelector);
				password.Clear();
				password.SendKeys(Password);
				Thread.Sleep(1000);

				var submit = FindElement(Driver, SubmitSelector);
				submit.Click();
				Thread.Sleep(5000);

				return true;
			}
			catch (Exception ex)
			{
				Log.Logger.Error($"LoginHandler failed: {ex}.");
				return false;
			}
		}

		private IWebElement FindElement(RemoteWebDriver webDriver, SelectorAttribute element)
		{
			switch (element.Type)
			{

				case SelectorType.XPath:
					{
						return webDriver.FindElementByXPath(element.Expression);
					}
				case SelectorType.Css:
					{
						return webDriver.FindElementByCssSelector(element.Expression);
					}
			}
			throw new SpiderException("Unsport findy: " + element.Type);
		}
	}

	/// <summary>
	/// 手动登录操作的实现
	/// </summary>
	public class ManualLoginHandler : WebDriverLoginHandler
	{
		/// <summary>
		/// 登陆的链接
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="url">登陆链接</param>
		public ManualLoginHandler(string url)
		{
			Url = url;
		}

		/// <summary>
		/// 登录操作的实现
		/// </summary>
		/// <returns>是否登录成功</returns>
		protected override bool Login()
		{
			try
			{
				Driver.Navigate().GoToUrl(Url);
				while (!Driver.Url.Contains("baidu.com"))
				{
					Thread.Sleep(1000);
				}
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
