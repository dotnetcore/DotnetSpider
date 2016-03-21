#if !NET_CORE

using OpenQA.Selenium;

namespace Java2Dotnet.Spider.Extension.Downloader.WebDriver
{
	public class WebDriverItem
	{
		public WebDriverItem(IWebDriver webDriver)
		{
			WebDriver = webDriver;
		}

		public IWebDriver WebDriver { get; }

		public override int GetHashCode()
		{
			return WebDriver.GetHashCode();
		}

		public override bool Equals(object o)
		{
			if (this == o) return true;
			if (o == null || GetType() != o.GetType()) return false;

			WebDriverItem request = (WebDriverItem)o;

			if (!WebDriver.Equals(request.WebDriver)) return false;

			return true;
		}
	}
}

#endif