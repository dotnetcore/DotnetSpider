
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
		private static RedisRedialManager _instanse;

		private const string RedialStatusKey = "REDIAL_STATUS";

		public string RedisHost { get; set; } = "localhost";
		private const string RunningRedialStatus = "Running";
		private const string DialingRedialStatus = "Dialing";
		private ConnectionMultiplexer redis;
        
        public static string SetPrefix { get; set; } = DateTime.Now.ToString() + "| ";
        public static string HostName { get; set; } = Dns.GetHostName();

		public override IAtomicExecutor AtomicExecutor { get; }

		private RedisRedialManager()
		{
            #if !NET_CORE
			var tmpRedisHost = ConfigurationManager.AppSettings["redialRedisServer"];
            #endif
            
            #if NET_CORE
            var tmpRedisHost = "192.168.199.202";
            #endif
            
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

			AtomicExecutor = new RedisAtomicExecutor(this);
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
        
		public override void WaitforRedialFinish()
		{            
			if (Skip)
			{
				return;
			}

            IDatabase db = redis.GetDatabase(0);
            RedisValue[] values = db.SetMembers(GetSetKey());
		    foreach (var v in values)
			{
				string key = v.ToString();
                
				//string dateTime = key.Split('|')[0];
				//string date = dateTime.Split(' ')[0];
				//string time = dateTime.Split(' ')[1];
				//int year = Convert.ToInt32(date.Split('-')[0]);
				//int month = Convert.ToInt32(date.Split('-')[1]);
				//int day = Convert.ToInt32(date.Split('-')[2]);
				//int hour = Convert.ToInt32(time.Split(':')[0]);
				//int minute = Convert.ToInt32(time.Split(':')[1]);
				//int second = Convert.ToInt32(time.Split(':')[2]);
                
				DateTime dt = DateTime.Parse(key);
				var ts = DateTime.Now - dt;
				double h = ts.TotalHours;
				if (h > 1)
				{
    				db.HashDelete(GetSetKey(),key);
				}
			}
            
			while (db.SetContains(GetSetKey(),"redial-lock"))
            {
                Thread.Sleep(50);
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

		public override RedialResult Redial()
		{
			if (Skip)
			{
				return RedialResult.Skip;
			}

            IDatabase db = redis.GetDatabase(0);
			if (db.SetContains(GetSetKey(),"redial-lock"))
			{
				while (true)
				{
					Thread.Sleep(50);
					if (!db.SetContains(GetSetKey(),"redial-lock"))
					{
						return RedialResult.OtherRedialed;
					}
				}
			}
			else
			{
				db.HashSet(GetSetKey(),"redial-lock","redialing");

				// wait all operation stop.
				Thread.Sleep(5000);

				AtomicExecutor.WaitAtomicAction();

				Logger.Warn("Try to redial network...");

				RedialInternet();

				db.HashDelete(GetSetKey(),"redial-lock");

				Logger.Warn("Redial finished.");
				return RedialResult.Sucess;
			}
		}

		public void Dispose()
		{
			redis?.Dispose();
		}
        
        public static string GetSetKey()
		{
			return HostName;
		}

		public static string GetFieldKey(string identity)
		{
			return SetPrefix + identity;
		}
	}
}