#if !NET_CORE

using System;
using System.Configuration;
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
		private static RedisRedialManager _instanse;

		private const string RedialStatusKey = "REDIAL_STATUS";

		public string RedisHost { get; set; } = "localhost";
		private const string RunningRedialStatus = "Running";
		private const string DialingRedialStatus = "Dialing";
		private ConnectionMultiplexer redis;

		public override IAtomicExecutor AtomicExecutor { get; }

		private RedisRedialManager()
		{
			var tmpRedisHost = ConfigurationManager.AppSettings["redialRedisServer"];
			if (!string.IsNullOrEmpty(tmpRedisHost))
			{
				RedisHost = tmpRedisHost;
			}
			else
			{
				throw new RedialException("Redial Redis Server did not set.");
			}

			redis = ConnectionMultiplexer.Connect(new ConfigurationOptions()
			{
				ServiceName = RedisHost,
				ConnectTimeout = 5000,
				KeepAlive = 8,
				EndPoints =
				{
					{ RedisHost, 6379 }
				}
			});

			var redialSetting = GetRedialStatus();
			if (redialSetting == null)
			{
				SetRedialStatus(RunningRedialStatus);
			}

			AtomicExecutor = new FileLockerAtomicExecutor(this);
		}

		public static RedisRedialManager Default
		{
			get
			{
				if (_instanse == null)
				{
					_instanse = new RedisRedialManager();
				}
				return _instanse;
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WaitforRedialFinish()
		{
			if (Skip)
			{
				return;
			}

			var redialSetting = GetRedialStatus();

			if (RunningRedialStatus != redialSetting)
			{
				while (true)
				{
					Thread.Sleep(50);
					var redialSetting1 = GetRedialStatus();
					if (redialSetting1 == RunningRedialStatus)
					{
						break;
					}
				}
			}
		}

		private string GetRedialStatus()
		{
			IDatabase db = redis.GetDatabase(0);
			return db.StringGet(RedialStatusKey);
		}

		private void SetRedialStatus(string value)
		{
			IDatabase db = redis.GetDatabase(0);
			db.StringSet(RedialStatusKey, value);

		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override RedialResult Redial()
		{
			if (Skip)
			{
				return RedialResult.Skip;
			}

			var redialSetting = GetRedialStatus();
			if (RunningRedialStatus != redialSetting)
			{
				while (true)
				{
					Thread.Sleep(50);
					var redialSetting1 = GetRedialStatus();
					if (redialSetting1 == RunningRedialStatus)
					{
						return RedialResult.OtherRedialed;
					}
				}
			}
			else
			{
				SetRedialStatus(DialingRedialStatus);

				// wait all operation stop.
				Thread.Sleep(5000);

				AtomicExecutor.WaitAtomicAction();

				Logger.Warn("Try to redial network...");

				RedialInternet();

				SetRedialStatus(RunningRedialStatus);

				Logger.Warn("Redial finished.");
				return RedialResult.Sucess;
			}
		}

		public void Dispose()
		{
			redis?.Dispose();
		}
	}
}
#endif