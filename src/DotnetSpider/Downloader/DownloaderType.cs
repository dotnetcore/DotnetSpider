namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 下载器类型
	/// </summary>
    public enum DownloaderType
    {
        Empty,
        HttpClient,
        WebDriver,
        ChromeExtension,
        Test,
        Exception
    }
}