using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace DotnetSpider.Redial.InternetDetector
{
	/// <summary>
	/// VPS 有多根线路, 其中几根是用于稳定远程, 另几根是IP拨号, 所以不能用PING baidu.com这种形式判断是否拨号成功.
	/// </summary>
	public class VpsInternetDetector : BaseInternetDetector
	{
		private readonly int _networkCount;

		public VpsInternetDetector()
		{
			_networkCount = 2;
			Timeout = 100;
		}
		public VpsInternetDetector(int networkCount, int maxWaitTime)
		{
			_networkCount = networkCount;
			Timeout = maxWaitTime;
		}

		public VpsInternetDetector(int networkCount = 2)
		{
			_networkCount = networkCount;
			Timeout = 100;
		}

		public override bool DoValidate()
		{
			return GetIp4Count() == _networkCount;
		}

		private int GetIp4Count()
		{
			string hostName = Dns.GetHostName();
#if !NET_CORE
			IPAddress[] addressList = Dns.GetHostAddresses(hostName);
#else
			IPAddress[] addressList = Dns.GetHostAddressesAsync(hostName).Result;
#endif
			return addressList.Count(i => i.AddressFamily == AddressFamily.InterNetwork);
		}
	}
}
