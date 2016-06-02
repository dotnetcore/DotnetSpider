#if !NET_CORE
using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Redial.Utils;

namespace Java2Dotnet.Spider.Redial.NetworkValidater
{
	public class VpnNetworkValidater : INetworkValidater
	{
		private readonly string _vpnInterface;

		public VpnNetworkValidater(string vpnInterface = "VPN连接")
		{
			_vpnInterface = vpnInterface;
		}

		public void Wait()
		{
			//在VpnRedialer中实现，因为操作Vpn断开和连接不那么稳定，所以在重拨过程中实现了重试，到Validate阶段已经保证网络通畅

			//bool stop = false;
			//while (!stop)
			//{
			//	try
			//	{
			//		VpnUtils util = new VpnUtils();
			//		stop = util.GetCurrentConnectingVpnNames().Contains(_vpnInterface);
			//	}
			//	catch
			//	{
			//		// ignored
			//	}
			//}
		}
	}
}
#endif
