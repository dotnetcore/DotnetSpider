using System;
using System.Net;
using System.Threading;
using DotnetSpider.Redial.InternetDetector;
using DotnetSpider.Redial.Redialer;
using StackExchange.Redis;
#if NET_CORE
using System.Linq;
using System.Runtime.InteropServices;
#endif

namespace DotnetSpider.Redial
{
	public class RedisRedialExecutor : RedialExecutor
	{
		protected IDatabase Db { get; set; }
		public static string HostName { get; set; } = Dns.GetHostName();
		public const string Locker = "redial-locker";

		public string Host { get; set; }
		public string Password { get; set; }
		public int Port { get; set; } = 6379;

		public RedisRedialExecutor(string host, string password, int port, IRedialer redialer, IInternetDetector validater) : base(redialer, validater)
		{
			Host = host;
			Password = password;
			Port = port;
		}

		public override void WaitAll()
		{
			while (true)
			{
				ClearTimeoutAction();
				if (Db.HashLength(HostName) <= 1)
				{
					break;
				}

				Thread.Sleep(50);
			}
		}

		public override void WaitRedialExit()
		{
			while (Db.HashExists(HostName, Locker))
			{
				ClearTimeoutRedialLocker();
				Thread.Sleep(50);
			}
		}

		public override string CreateActionIdentity(string name)
		{
			string identity = name + "_" + Guid.NewGuid().ToString("N");
			Db.HashSet(HostName, identity, DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
			return identity;
		}

		public override void DeleteActionIdentity(string identity)
		{
			Db.HashDelete(HostName, identity);
		}

		public override bool CheckIsRedialing()
		{
			if (Db.HashExists(HostName, Locker))
			{
				Db.HashSet(HostName, Locker, DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
				return false;
			}
			return true;
		}

		public override void ReleaseRedialLocker()
		{
			Db.HashDelete(HostName, Locker);
		}

		private void ClearTimeoutAction()
		{
			foreach (var entry in Db.HashGetAll(HostName))
			{
				string key = entry.Name;
				string value = entry.Value;

				DateTime dt = DateTime.Parse(value);
				var minutes = (DateTime.Now - dt).TotalMinutes;

				if (minutes > 5)
				{
					Db.HashDelete(HostName, key);
				}
			}
		}

		private void ClearTimeoutRedialLocker()
		{
			var result = Db.HashGet(HostName, Locker);
			if (result.HasValue)
			{
				var value = DateTime.Parse(result.ToString());
				var minutes = (DateTime.Now - value).TotalMinutes;

				if (minutes > 5)
				{
					Db.HashDelete(HostName, Locker);
				}
			}
		}

		public override void Init()
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

			Db = redis.GetDatabase(3);
		}
	}
}
