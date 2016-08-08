#if !NET_CORE
using System;
using DotnetSpider.Redial.Utils;
using System.Threading;

namespace DotnetSpider.Redial.Redialer
{
	public class VpnRedialer : IRedialer
	{
		public string NetInterface { get; set; }
		private readonly VpnUtils _util;
		public VpnRedialer(string interface1, string vpnIp, string user, string password, string netInterface = null)
		{
			_util = new VpnUtils(vpnIp, interface1, user, password);
			NetInterface = netInterface;
		}

		public void Redial()
		{
			Console.WriteLine("Trying to disconnect Vpn:" + _util.VpnName);
			_util.TryDisConnectVpn();
			Thread.Sleep(1000);
			Console.WriteLine("Finish disconnect:" + _util.VpnName);

			//在NAT下使用VPN，断开VPN时会导致原网络断开，需要重启网卡，暂时没有找到更好的解决方法
			if (!string.IsNullOrEmpty(NetInterface))
			{
				NetInterfaceUtils.ChangeNetworkConnectionStatus(false, NetInterface);
				Thread.Sleep(2000);
				NetInterfaceUtils.ChangeNetworkConnectionStatus(true, NetInterface);
				Thread.Sleep(2000);
			}

			Console.WriteLine("Trying to connect Vpn:" + _util.VpnName);
			_util.TryConnectVpn();
			Thread.Sleep(1000);
			Console.WriteLine("Finish connect:" + _util.VpnName);
		}
	}
}
#endif

