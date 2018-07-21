using OpenQA.Selenium.Remote;
using System.Net;

namespace DotnetSpider.Extension
{
	public static class WebDriverExtensions
	{
		public static CookieCollection GetCookieCollection(this RemoteWebDriver driver)
		{
			var cookies = new CookieCollection();

			foreach (var cookie in driver.Manage().Cookies.AllCookies)
			{
				cookies.Add(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
			}
			return cookies;
		}
	}
}
