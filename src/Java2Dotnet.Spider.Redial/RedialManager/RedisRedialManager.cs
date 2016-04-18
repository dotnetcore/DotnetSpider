
using System;
#if !NET_CORE
using System.Configuration;
#endif
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using Java2Dotnet.Spider.Redial.AtomicExecutor;
using RedisSharp;

namespace Java2Dotnet.Spider.Redial.RedialManager
{
	/// <summary>
	/// 用于一台电脑一根拨号线路或多台共用一个交换机一个拨号线路
	/// </summary>
	public class RedisRedialManager : BaseRedialManager, IDisposable
	{
		public string RedisHost { get; set; }
		public override IAtomicExecutor AtomicExecutor { get; }

		private string HostName { get; }
		private string Password { get; }

		public const string Locker = "redial-locker";
		public RedisServer Redis { get; }

		public RedisRedialManager(string host, string password)
		{
			if (!string.IsNullOrEmpty(host))
			{
				RedisHost = host;
			}
			else
			{
				RedisHost = "localhost";
			}

			if (!string.IsNullOrEmpty(password))
			{
				Password = password;
			}
			else
			{
				Password = null;
			}
			Redis = new RedisServer(host, 6379, password);
			Redis.Db = 3;
			AtomicExecutor = new RedisAtomicExecutor(this);
		}

		public RedisRedialManager() : this(Common.ConfigurationManager.Get("redialRedisHost"), Common.ConfigurationManager.Get("redialRedisPassword"))
		{
		}

		public static RedisRedialManager Create(string host)
		{
			return new RedisRedialManager(host, null);
		}

		public static RedisRedialManager Create(string host, string password)
		{
			return new RedisRedialManager(host, password);
		}

		public override void WaitforRedialFinish()
		{
			if (Skip)
			{
				return;
			}

			while (Redis.HashExists(GetSetKey(), Locker))
			{
				ClearTimeoutLocker();
				Thread.Sleep(50);
			}
		}

		private void ClearTimeoutLocker()
		{
			var result = Redis.HashGet(GetSetKey(), Locker);
			if (result == null)
			{
				return;
			}
			else
			{
				var value = DateTime.Parse(result.ToString());
				var minutes = (DateTime.Now - value).TotalMinutes;

				if (minutes > 5)
				{
					Redis.HashDelete(GetSetKey(), Locker);
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

			if (Redis.HashExists(GetSetKey(), Locker))
			{
				while (true)
				{
					Thread.Sleep(50);
					if (!Redis.HashExists(GetSetKey(), Locker))
					{
						return RedialResult.OtherRedialed;
					}
				}
			}
			else
			{
				Redis.HashSet(GetSetKey(), Locker, DateTime.Now.ToString("yyyy-MM-dd hh:mm"));

				// wait all operation stop.
				Thread.Sleep(5000);

				AtomicExecutor.WaitAtomicAction();

				Logger.Warn("Try to redial network...");

				RedialInternet();

				Redis.HashDelete(GetSetKey(), Locker);

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