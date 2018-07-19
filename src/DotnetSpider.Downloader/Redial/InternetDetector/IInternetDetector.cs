namespace DotnetSpider.Downloader.Redial.InternetDetector
{
	/// <summary>
	/// 网络状态检测器
	/// </summary>
	public interface IInternetDetector
	{
		/// <summary>
		/// 超时时间
		/// </summary>
		int Timeout { get; set; }

		/// <summary>
		/// 检测网络状态
		/// </summary>
		/// <returns>如果返回 True, 表示当前可以访问互联网</returns>
		bool Detect();
	}
}
