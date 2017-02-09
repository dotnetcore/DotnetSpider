#if !NET_CORE
using OpenQA.Selenium.Remote;

namespace DotnetSpider.Extension.Downloader.WebDriver
{
	public interface IWebDriverHandler
	{
		bool Handle(RemoteWebDriver driver);
	}
}
#endif