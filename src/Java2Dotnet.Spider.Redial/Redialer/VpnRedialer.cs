#if !NET_CORE
using Java2Dotnet.Spider.Redial.Utils;
using System.Threading;

namespace Java2Dotnet.Spider.Redial.Redialer
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
			_util.TryDisConnectVpn();
			Thread.Sleep(1000);

			while (_util.GetCurrentConnectingVpnNames().Contains(_util.VpnName))
			{
				_util.TryDisConnectVpn();
				Thread.Sleep(2000);
			}

			//在NAT下使用VPN，断开VPN时会导致原网络断开，需要重启网卡，暂时没有找到更好的解决方法
			if (!string.IsNullOrEmpty(NetInterface))
			{
				NetInterfaceUtils.ChangeNetworkConnectionStatus(false, NetInterface);
				Thread.Sleep(2000);
				NetInterfaceUtils.ChangeNetworkConnectionStatus(true, NetInterface);
				Thread.Sleep(2000);
			}

			_util.TryConnectVpn();
			Thread.Sleep(1000);

			while (!_util.GetCurrentConnectingVpnNames().Contains(_util.VpnName))
			{
				try
				{
					_util.TryConnectVpn();
					Thread.Sleep(10000);
				}
				catch
				{
					// ignored
				}
			}
		}
	}
}
#endif

