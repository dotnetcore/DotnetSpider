#if !NET_CORE
using System;
using DotnetSpider.Extension.Infrastructure;
using System.Threading;
using DotnetSpider.Core.Redial.Redialer;

namespace DotnetSpider.Extension.Redial.Redialer
{
	/// <summary>
	/// 在任务机器上使用VPN连接网络时使用的拨号器
	/// </summary>
	public class VpnRedialer : IRedialer
	{
		private readonly string _vpnInterface;
		private readonly string _vpnIp;
		private readonly string _account;
		private readonly string _password;
		private readonly VpnRedial _vpnRedial;
		private readonly string _netInterface;

		/// <summary>
		/// 构造方法
		/// </summary>
		public VpnRedialer(string vpnInterface, string vpnIp, string user, string password, string netInterface = null)
		{
			_vpnInterface = vpnInterface;
			_vpnIp = vpnIp;
			_account = user;
			_password = password;
			_netInterface = netInterface;
			_vpnRedial = new VpnRedial(vpnIp, vpnInterface, user, password);
		}

		/// <summary>
		/// 拨号
		/// </summary>
		public void Redial()
		{
			Console.WriteLine("Trying to disconnect Vpn:" + _vpnRedial.VpnName);
			_vpnRedial.TryDisConnectVpn();
			Thread.Sleep(1000);
			Console.WriteLine("Finish disconnect:" + _vpnRedial.VpnName);

			//在NAT下使用VPN，断开VPN时会导致原网络断开，需要重启网卡，暂时没有找到更好的解决方法
			if (!string.IsNullOrEmpty(_netInterface))
			{
				NetInterfaceUtil.ChangeNetworkConnectionStatus(false, _netInterface);
				Thread.Sleep(2000);
				NetInterfaceUtil.ChangeNetworkConnectionStatus(true, _netInterface);
				Thread.Sleep(2000);
			}

			Console.WriteLine("Trying to connect Vpn:" + _vpnRedial.VpnName);
			_vpnRedial.TryConnectVpn();
			Thread.Sleep(1000);
			Console.WriteLine("Finish connect:" + _vpnRedial.VpnName);
		}
	}
}
#endif

