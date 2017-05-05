using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace DotnetSpider.Extension.Redial.InternetDetector
{
	/// <summary>
	/// VPS 有多根线路, 其中几根是用于稳定远程, 另几根是IP拨号, 所以不能用PING baidu.com这种形式判断是否拨号成功.
	/// </summary>
	public class VpsInternetDetector : BaseInternetDetector
	{
		public int NetworkCount { get; set; }

		public VpsInternetDetector()
		{
			NetworkCount = 2;
			Timeout = 100;
		}
		public VpsInternetDetector(int networkCount, int maxWaitTime)
		{
			NetworkCount = networkCount;
			Timeout = maxWaitTime;
		}

		public VpsInternetDetector(int networkCount = 2)
		{
			NetworkCount = networkCount;
			Timeout = 100;
		}

		public override bool DoValidate()
		{
			return GetIp4Count() == NetworkCount;
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
