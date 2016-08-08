using DotnetSpider.Redial.NetworkValidater;
using System;

namespace DotnetSpider.Extension.Configuration
{
	public abstract class NetworkValidater
	{
		public int MaxWaitTime { get; set; } = 10;
		[Flags]
		public enum Types
		{
			Defalut,
			Vps,
			Vpn
		}

		public abstract Types Type { get; internal set; }

		public abstract INetworkValidater GetNetworkValidater();
	}

	public class DefaultNetworkValidater : NetworkValidater
	{
		public override Types Type { get; internal set; } = Types.Defalut;

		public override INetworkValidater GetNetworkValidater()
		{
			return new Redial.NetworkValidater.DefaultNetworkValidater(MaxWaitTime);
		}
	}

	public class VpsNetworkValidater : NetworkValidater
	{
		public override Types Type { get; internal set; } = Types.Vps;

		public int InterfaceNum { get; set; } = 2;

		public override INetworkValidater GetNetworkValidater()
		{
			return new Redial.NetworkValidater.VpsNetworkValidater(InterfaceNum, MaxWaitTime);
		}
	}

#if !NET_CORE
	public class VpnNetworkValidater : NetworkValidater
	{
		public override Types Type { get; internal set; } = Types.Vpn;

		public string VpnName { get; set; } = "VPN连接";

		public override INetworkValidater GetNetworkValidater()
		{
			return new Redial.NetworkValidater.VpnNetworkValidater(VpnName);
		}
	}
#endif
}
