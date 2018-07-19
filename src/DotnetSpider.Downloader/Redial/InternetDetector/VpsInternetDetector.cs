using System.Net;
using System.Net.Sockets;

namespace DotnetSpider.Downloader.Redial.InternetDetector
{
	/// <summary>
	/// 自建VPS 可能有多根线路, 其中几根是用于稳定远程, 另几根是IP拨号, 所以不能用PING baidu.com这种形式判断是否拨号成功.
	/// </summary>
	public class VpsInternetDetector : InternetDetector
	{
		private readonly int _networkCount;

		/// <summary>
		/// 构造方法
		/// </summary>
		public VpsInternetDetector()
		{
			_networkCount = 2;
			Timeout = 100;
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="networkCount">多少个网络接口数量则为网络正常</param>
		/// <param name="timeout"></param>
		public VpsInternetDetector(int networkCount, int timeout)
		{
			_networkCount = networkCount;
			Timeout = timeout;
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="networkCount">多少个网络接口数量则为网络正常</param>
		public VpsInternetDetector(int networkCount = 2)
		{
			_networkCount = networkCount;
			Timeout = 100;
		}

		/// <summary>
		/// 检测网络状态
		/// </summary>
		/// <returns>如果返回 True, 表示当前可以访问互联网</returns>
		protected override bool DoValidate()
		{
			return GetIp4Count() == _networkCount;
		}

		private int GetIp4Count()
		{
			string hostName = Dns.GetHostName();
			var addressList = Dns.GetHostAddresses(hostName);
			int count = 0;
			foreach (var address in addressList)
			{
				if (address.AddressFamily == AddressFamily.InterNetwork)
				{
					count++;
				}
			}
			return count;
		}
	}
}
