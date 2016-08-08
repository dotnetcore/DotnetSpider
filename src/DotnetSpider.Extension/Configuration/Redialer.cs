using System;
using DotnetSpider.Redial.Redialer;
using Newtonsoft.Json;
using DotnetSpider.Redial.RedialManager;
using DotnetSpider.Redial;
using DotnetSpider.Redial.NetworkValidater;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Configuration
{
	public abstract class Redialer
	{
		[Flags]
		public enum Types
		{
			Adsl,
			H3C,
			Vpn
		}

		[JsonIgnore]
		public NetworkValidater NetworkValidater { get; set; }

		public RedialManager RedialManager { get; set; }

		public abstract Types Type { get; internal set; }

		public abstract IRedialer GetRedialer();
	}

	public abstract class RedialManager
	{
		[Flags]
		public enum Types
		{
			Redis,
			File
		}

		public abstract Types Type { get; internal set; }
		public abstract void SetRedialManager(INetworkValidater networkValidater, IRedialer redialer);
	}

	public class FileRedialManager : RedialManager
	{
		public override Types Type { get; internal set; } = Types.File;

		public override void SetRedialManager(INetworkValidater networkValidater, IRedialer redialer)
		{
			NetworkProxyManager.Current.Register(new RedialExecutor(new FileLockerRedialManager(networkValidater, redialer)));
		}
	}

	public class RedisRedialManager : RedialManager
	{
		public string Host { get; set; }
		public string Password { get; set; }
		public int Port { get; set; }

		public override Types Type { get; internal set; } = Types.Redis;

		public override void SetRedialManager(INetworkValidater networkValidater, IRedialer redialer)
		{
			NetworkProxyManager.Current.Register(new RedialExecutor(new Redial.RedialManager.RedisRedialManager(Host, Password, networkValidater, redialer)));
		}
	}

	public class AdslRedialer : Redialer
	{
		public string Interface { get; set; } = "宽带连接";
		public string Account { get; set; }
		public string Password { get; set; }

		public override Types Type { get; internal set; } = Types.Adsl;

		public override IRedialer GetRedialer()
		{
			return new Redial.Redialer.AdslRedialer(Interface, Account, Password);
		}
	}

#if !NET_CORE
	public class H3CRedialer : Redialer
	{
		public string Sshhost;
		public string Sshuser;
		public string Sshpass;
		public string SshInterface;
		public int Sshport;
		public string Account { get; set; }
		public string Password { get; set; }

		public override Types Type { get; internal set; } = Types.H3C;

		public override IRedialer GetRedialer()
		{
			return new H3CSshAdslRedialer(Sshhost, Sshport, Sshuser, Sshpass, SshInterface, Account, Password);
		}
	}
#endif

#if !NET_CORE
	public class VpnRedialer : Redialer
	{
		public string NetInterface { get; set; } = null;
		public string VpnInterface { get; set; }
		public string VpnIp { get; set; }
		public string Account { get; set; }
		public string Password { get; set; }

		public override Types Type { get; internal set; } = Types.Vpn;

		public override IRedialer GetRedialer()
		{
			return new Redial.Redialer.VpnRedialer(VpnInterface, VpnIp, Account, Password, NetInterface);
		}
	}
#endif
}
