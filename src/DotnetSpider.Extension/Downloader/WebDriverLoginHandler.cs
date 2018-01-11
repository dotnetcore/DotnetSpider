using DotnetSpider.Core;
using System;
using DotnetSpider.Core.Selector;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System.Threading;
using DotnetSpider.Extension.Model;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Downloader;
using System.Net;

namespace DotnetSpider.Extension.Downloader
{
	/// <summary>
	/// 通过WebDriver的登录操作的抽象
	/// </summary>
	public abstract class WebDriverLoginHandler : LoginHandler
	{
		public RemoteWebDriver Driver { get; set; }

		public WebDriverLoginHandler()
		{
		}

		protected override CookieCollection GetCookies(ISpider spider)
		{
			return new CookieCollection();
		}

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

		protected abstract bool Login();
	}

	public class CommonLoginHandler : WebDriverLoginHandler
	{
		public string Url { get; set; }

		public Selector UserSelector { get; set; }

		public string User { get; set; }

		public Selector PassSelector { get; set; }

		public string Password { get; set; }

		public Selector SubmitSelector { get; set; }

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

				var password = FindElement(Driver, PassSelector);
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
				Logger.Log($"LoginHandler failed: {ex}.", Level.Error);
				return false;
			}
		}

		private IWebElement FindElement(RemoteWebDriver webDriver, Selector element)
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

	public class ManualLoginHandler : WebDriverLoginHandler
	{
		public Uri Url { get; set; }

		public ManualLoginHandler(string url)
		{
			Url = new Uri(url);
		}

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
