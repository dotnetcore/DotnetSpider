using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using DotnetSpider.Core;
using System.Net;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction;
using DotnetSpider.Common;

namespace DotnetSpider.Extension.Downloader
{
	/// <summary>
	/// WebDriver 的Cookie注入器
	/// </summary>
	public class WebDriverCommonCookieInjector : WebDriverCookieInjector
	{
		/// <summary>
		/// 登陆的链接
		/// </summary>
		public string Url;

		/// <summary>
		/// 登陆成功后需要再次导航到的链接
		/// </summary>
		public string AfterLoginUrl;

		/// <summary>
		/// 用户名在网页中的元素选择器
		/// </summary>
		public Selector UserSelector;

		/// <summary>
		/// 用户名
		/// </summary>
		public string User;

		/// <summary>
		/// 密码在网页中的元素选择器
		/// </summary>
		public Selector PasswordSelector;

		/// <summary>
		/// 密码
		/// </summary>
		public string Password;

		/// <summary>
		/// 登陆按钮的元素选择器
		/// </summary>
		public Selector SubmitSelector;

		public WebDriverCommonCookieInjector(Browser browser, IControllable controllable) : base(browser, controllable)
		{
		}


		/// <summary>
		/// 在输入用户信息前执行的一些准备操作
		/// </summary>
		/// <param name="webDriver">WebDriver</param>
		protected virtual void BeforeInput(RemoteWebDriver webDriver)
		{
		}

		/// <summary>
		/// 完成登陆后执行的一些准备操作
		/// </summary>
		/// <param name="webDriver"></param>
		protected virtual void AfterLogin(RemoteWebDriver webDriver)
		{
		}

		/// <summary>
		/// 查找元素
		/// </summary>
		/// <param name="webDriver">WebDriver</param>
		/// <param name="element">页面元素选择器</param>
		/// <returns>页面元素</returns>
		protected IWebElement FindElement(RemoteWebDriver webDriver, Selector element)
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

			throw new SpiderException("Unsport findby: " + element.Type);
		}

		/// <summary>
		/// 取得 Cookie
		/// </summary>
		/// <param name="controllable">可控制程序 <see cref="IControllable"/></param>
		/// <returns>Cookies <see cref="CookieCollection"/></returns>
		protected override CookieCollection GetCookies(IControllable controllable)
		{
			if (string.IsNullOrEmpty(User) || string.IsNullOrEmpty(Password) || UserSelector == null || PasswordSelector == null)
			{
				throw new SpiderException("Arguments of WebDriverCookieInjector are incorrect.");
			}

			var driver = CreateWebDriver();

			try
			{
				driver.Navigate().GoToUrl(Url);
				Thread.Sleep(10000);

				BeforeInput(driver);

				if (UserSelector != null)
				{
					var user = FindElement(driver, UserSelector);
					user.Clear();
					user.SendKeys(User);
					Thread.Sleep(1500);
				}

				if (PasswordSelector != null)
				{
					var pass = FindElement(driver, PasswordSelector);
					pass.Clear();
					pass.SendKeys(Password);
					Thread.Sleep(1500);
				}

				var submit = FindElement(driver, SubmitSelector);
				submit.Click();
				Thread.Sleep(10000);

				AfterLogin(driver);

				return driver.GetCookieCollection();
			}
			finally
			{
				driver.Dispose();
			}
		}
	}
}
