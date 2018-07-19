using OpenQA.Selenium.Remote;

namespace DotnetSpider.Extension.Downloader
{
	/// <summary>
	/// WebDriver的操作, 如滚动、点击
	/// </summary>
	public interface IWebDriverAction
	{
		/// <summary>
		/// 实现操作 WebDriver
		/// </summary>
		/// <param name="driver">WebDriver</param>
		/// <returns>是否操作成功</returns>
		bool Invoke(RemoteWebDriver driver);
	}
}
