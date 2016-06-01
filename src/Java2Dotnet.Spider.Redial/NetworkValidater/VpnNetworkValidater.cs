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
			bool stop = false;
			while (!stop)
			{
				try
				{
					VpnUtils util = new VpnUtils();
					stop = util.GetCurrentConnectingVpnNames().Contains(_vpnInterface);
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
