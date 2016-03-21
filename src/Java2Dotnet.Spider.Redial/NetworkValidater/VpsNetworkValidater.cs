using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Java2Dotnet.Spider.Redial.NetworkValidater
{
	/// <summary>
	/// VPS 有多根线路, 其中几根是用于稳定远程, 另几根是IP拨号, 所以不能用PING baidu.com这种形式判断是否拨号成功.
	/// </summary>
	public class VpsNetworkValidater : INetworkValidater
	{
		private readonly int _networkCount;

		public VpsNetworkValidater(int networkCount = 2)
		{
			_networkCount = networkCount;

			// add static router 
		}

		public void Wait()
		{
			while (true)
			{
				if (GetIp4Count() == _networkCount)
				{
					break;
				}

				Console.WriteLine("VPS Waiter is waiting...");
				Thread.Sleep(200);
			}
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
