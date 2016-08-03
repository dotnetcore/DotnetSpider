
using System;
#if !NET_CORE
using System.Configuration;
#endif
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using Java2Dotnet.Spider.Redial.AtomicExecutor;
using Java2Dotnet.Spider.Log;
using StackExchange.Redis;
using Java2Dotnet.Spider.Redial.NetworkValidater;
using Java2Dotnet.Spider.Redial.Redialer;
using System.Linq;
using System.Runtime.InteropServices;

namespace Java2Dotnet.Spider.Redial.RedialManager
{
	/// <summary>
	/// 用于一台电脑一根拨号线路或多台共用一个交换机一个拨号线路
	/// </summary>
	public class RedisRedialManager : BaseRedialManager, IDisposable
	{
		public string RedisHost { get; set; }
		public override IAtomicExecutor AtomicExecutor { get; }
		private string HostName { get; } = Dns.GetHostName();
		private string Password { get; }
		public const string Locker = "redial-locker";
		public IDatabase Db { get; }

		public RedisRedialManager(string host, string password, INetworkValidater validater, IRedialer redialer, ILogService logger)
		{
			Logger = logger;
			if (!string.IsNullOrEmpty(host))
			{
				RedisHost = host;
			}
			else
			{
				throw new RedialException("Redis host should not be null.");
			}

			if (!string.IsNullOrEmpty(password))
			{
				Password = password;
			}
			else
			{
				Password = null;
			}

			var confiruation = new ConfigurationOptions()
			{
				ServiceName = "DotnetSpider",
				Password = password,
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
				var address = Dns.GetHostAddressesAsync(host).Result.FirstOrDefault();
				if (address == null)
				{
					throw new Exception("Can't resovle your host: " + host);
				}
				confiruation.EndPoints.Add(new IPEndPoint(address, 6379));
			}
			else
			{
				confiruation.EndPoints.Add(new DnsEndPoint(host, 6379));
			}
#else
			confiruation.EndPoints.Add(new DnsEndPoint(host, 6379));
#endif
			var redis = ConnectionMultiplexer.Connect(confiruation);

			Db = redis.GetDatabase(3);
			AtomicExecutor = new RedisAtomicExecutor(this);
			NetworkValidater = validater;
			Redialer = redialer;
		}

		public RedisRedialManager(IDatabase db, INetworkValidater validater, IRedialer redialer, ILogService logger)
		{
			Logger = logger;

			Db = db;
			AtomicExecutor = new RedisAtomicExecutor(this);
			NetworkValidater = validater;
			Redialer = redialer;
		}

		public override void WaitforRedialFinish()
		{
			if (Skip)
			{
				return;
			}

			while (Db.HashExists(GetSetKey(), Locker))
			{
				ClearTimeoutLocker();
				Thread.Sleep(50);
			}
		}

		private void ClearTimeoutLocker()
		{
			var result = Db.HashGet(GetSetKey(), Locker);
			if (!result.HasValue)
			{
				return;
			}
			else
			{
				var value = DateTime.Parse(result.ToString());
				var minutes = (DateTime.Now - value).TotalMinutes;

				if (minutes > 5)
				{
					Db.HashDelete(GetSetKey(), Locker);
				}
			}
		}

		public override RedialResult Redial()
		{
			if (Skip)
			{
				return RedialResult.Skip;
			}

			ClearTimeoutLocker();

			if (Db.HashExists(GetSetKey(), Locker))
			{
				while (true)
				{
					Thread.Sleep(50);
					if (!Db.HashExists(GetSetKey(), Locker))
					{
						return RedialResult.OtherRedialed;
					}
				}
			}
			else
			{
				Db.HashSet(GetSetKey(), Locker, DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

				// wait all operation stop.
				Thread.Sleep(5000);

				AtomicExecutor.WaitAtomicAction();

				Logger?.Warn("Try to redial network...");

				RedialInternet();

				Db.HashDelete(GetSetKey(), Locker);

				Logger?.Warn("Redial finished.");
				return RedialResult.Sucess;
			}
		}

		public void Dispose()
		{
		}

		private string GetSetKey()
		{
			return HostName;
		}
	}
}