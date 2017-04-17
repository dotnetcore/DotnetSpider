#if !NET_CORE
namespace DotnetSpider.Extension.Redial.InternetDetector
{
	public class VpnInternetDetector : BaseInternetDetector
	{
		private readonly string _vpnInterface;

		public VpnInternetDetector(string vpnInterface = "VPN连接")
		{
			_vpnInterface = vpnInterface;
		}


		public override bool DoValidate()
		{
			//已经在VpnRedialer中实现验证，推荐Vpn拨号使用DefaultNetworkValidater
			return true;
		}
	}
}
#endif
