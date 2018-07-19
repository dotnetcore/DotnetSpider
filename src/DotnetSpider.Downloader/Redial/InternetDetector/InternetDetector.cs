using System.Threading;

namespace DotnetSpider.Downloader.Redial.InternetDetector
{
	/// <summary>
	/// 网络状态检测器
	/// </summary>
	public abstract class InternetDetector : IInternetDetector
	{
		/// <summary>
		/// 检测网络状态
		/// </summary>
		/// <returns>如果返回 True, 表示当前可以访问互联网</returns>
		protected abstract bool DoValidate();

		/// <summary>
		/// 超时时间
		/// </summary>
		public int Timeout { get; set; } = 10;

		/// <summary>
		/// 检测网络状态
		/// </summary>
		/// <returns>如果返回 True, 表示当前可以访问互联网</returns>
		public bool Detect()
		{
			int currentWaitTime = 0;
			while (currentWaitTime < Timeout)
			{
				currentWaitTime++;
				try
				{
					if (DoValidate())
					{
						return true;
					}

					if (currentWaitTime > 4)
					{
						return false;
					}

					Thread.Sleep(1500);
				}
				catch
				{
					// ignored
				}
			}
			return false;
		}
	}
}
