
using System;
#if !NET_CORE
using System.Configuration;
#endif
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using Java2Dotnet.Spider.Redial.AtomicExecutor;
using StackExchange.Redis;

namespace Java2Dotnet.Spider.Redial.RedialManager
{
	/// <summary>
	/// 用于一台电脑一根拨号线路或多台共用一个交换机一个拨号线路
	/// </summary>
	public class RedisRedialManager : BaseRedialManager, IDisposable
	{
		public string RedisHost { get; set; }
		public override IAtomicExecutor AtomicExecutor { get; }
		public IDatabase Db { get; }

		private string HostName { get; } = Dns.GetHostName();
		public const string Locker = "redial-locker";
		public ConnectionMultiplexer Redis { get; }

		private RedisRedialManager(string host)
		{
			if (!string.IsNullOrEmpty(host))
			{
				RedisHost = host;
			}
			else
			{
				RedisHost = "localhost";
			}

			Redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
			{
				ServiceName = RedisHost,
				ConnectTimeout = 5000,
				KeepAlive = 8,
#if !RELEASE
				AllowAdmin = true,
#endif
				EndPoints =
				{
					{ RedisHost, 6379 }
				}
			});

			Db = Redis.GetDatabase(1);

			AtomicExecutor = new RedisAtomicExecutor(this);
		}

		public static RedisRedialManager Create(string host)
		{
			return new RedisRedialManager(host);
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
				Db.HashSet(GetSetKey(), Locker, DateTime.Now.ToString("yyyy-MM-dd hh:mm"));

				// wait all operation stop.
				Thread.Sleep(5000);

				AtomicExecutor.WaitAtomicAction();

				Logger.Warn("Try to redial network...");

				RedialInternet();

				Db.HashDelete(GetSetKey(), Locker);

				Logger.Warn("Redial finished.");
				return RedialResult.Sucess;
			}
		}

		public void Dispose()
		{
			Redis?.Dispose();
		}

		private string GetSetKey()
		{
			return HostName;
		}
	}
}