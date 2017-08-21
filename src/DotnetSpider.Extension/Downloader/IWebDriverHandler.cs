
using OpenQA.Selenium.Remote;

namespace DotnetSpider.Extension.Downloader
{
	public interface IWebDriverHandler
	{
		bool Handle(RemoteWebDriver driver);
	}
}
