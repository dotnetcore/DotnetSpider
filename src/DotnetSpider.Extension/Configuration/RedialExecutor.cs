using System;
using System.Net;
#if NET_CORE
using System.Runtime.InteropServices;
using System.Linq;
#endif
using DotnetSpider.Redial.Redialer;
using DotnetSpider.Redial;
using DotnetSpider.Core;
using StackExchange.Redis;
using DotnetSpider.Redial.InternetDetector;

namespace DotnetSpider.Extension.Configuration
{
	#region Redialer

	public abstract class Redialer
	{
		[Flags]
		public enum Types
		{
			Adsl,
			H3C,
			Vpn
		}

		public abstract Types Type { get; internal set; }

		public abstract IRedialer GetRedialer();
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

	#endregion

	#region InternetDetector

	public abstract class InternetDetector
	{
		public int Timeout { get; set; } = 10;

		[Flags]
		public enum Types
		{
			Defalut,
			Vps,
			Vpn
		}

		public abstract Types Type { get; internal set; }

		public abstract IInternetDetector GetInternetDetector();
	}

	public class DefaultInternetDetector : InternetDetector
	{
		public override Types Type { get; internal set; } = Types.Defalut;

		public override IInternetDetector GetInternetDetector()
		{
			return new DefalutInternetDetector(Timeout);
		}
	}

	public class VpsInternetDetector : InternetDetector
	{
		public override Types Type { get; internal set; } = Types.Vps;

		public int InterfaceNum { get; set; } = 2;

		public override IInternetDetector GetInternetDetector()
		{
			return new Redial.InternetDetector.VpsInternetDetector(InterfaceNum, Timeout);
		}
	}

#if !NET_CORE
	public class VpnInternetDetector : InternetDetector
	{
		public override Types Type { get; internal set; } = Types.Vpn;

		public string VpnName { get; set; } = "VPN连接";

		public override IInternetDetector GetInternetDetector()
		{
			return new Redial.InternetDetector.VpnInternetDetector(VpnName);
		}
	}
#endif

	#endregion

	#region RedialExecutor
	public abstract class RedialExecutor
	{
		[Flags]
		public enum Types
		{
			Redis,
			File
		}

		public InternetDetector InternetDetector { get; set; }
		public Redialer Redialer { get; set; }

		public abstract Types Type { get; internal set; }

		public abstract INetworkExecutor GetRedialExecutor();
	}

	public class FileRedialExecutor : RedialExecutor
	{
		public override Types Type { get; internal set; } = Types.File;

		public override INetworkExecutor GetRedialExecutor()
		{
			return new FileLockerRedialExecutor(Redialer.GetRedialer(), InternetDetector.GetInternetDetector());
		}
	}

	public class RedisRedialExecutor : RedialExecutor
	{
		public string Host { get; set; }
		public string Password { get; set; }
		public int Port { get; set; } = 6379;

		public override Types Type { get; internal set; } = Types.Redis;

		public override INetworkExecutor GetRedialExecutor()
		{
			var confiruation = new ConfigurationOptions()
			{
				ServiceName = "DotnetSpider",
				Password = Password,
				ConnectTimeout = 5000,
				KeepAlive = 8,
				ConnectRetry = 20,
				SyncTimeout = 65530,
				ResponseTimeout = 65530
			};
#if NET_CORE
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				// Lewis: This is a Workaround for .NET CORE can't use EndPoint to create Socket.
				var address = Dns.GetHostAddressesAsync(Host).Result.FirstOrDefault();
				if (address == null)
				{
					throw new Exception("Can't resovle your host: " + Host);
				}
				confiruation.EndPoints.Add(new IPEndPoint(address, 6379));
			}
			else
			{
				confiruation.EndPoints.Add(new DnsEndPoint(Host, 6379));
			}
#else
			confiruation.EndPoints.Add(new DnsEndPoint(Host, 6379));
#endif
			var redis = ConnectionMultiplexer.Connect(confiruation);

			var db = redis.GetDatabase(3);
			return new Redial.RedisRedialExecutor(db, Redialer.GetRedialer(), InternetDetector.GetInternetDetector());
		}
	}
	#endregion
}
