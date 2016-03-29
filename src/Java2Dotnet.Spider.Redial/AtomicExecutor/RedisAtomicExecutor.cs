using System;
using System.Linq;
using System.Net;
using System.Threading;
using Java2Dotnet.Spider.Redial.RedialManager;
using Java2Dotnet.Spider.Redial.Utils;
using StackExchange.Redis;

namespace Java2Dotnet.Spider.Redial.AtomicExecutor
{
	internal class RedisAtomicExecutor : IAtomicExecutor
	{
		private static ConnectionMultiplexer _redis;
		private static IDatabase _db;
        
        public static string SetPrefix { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "| ";
        public static string HostName { get; set; } = Dns.GetHostName();
        
        public static string RedisHost { get; set;} = "192.168.199.202";

		public void Execute(string name, Action action)
		{
			WaitforRedial.WaitforRedialFinish();
            string setKey = GetSetKey();
            string fieldKey = GetFieldKey(name);

			try
			{
				//Db.SetAdd(setKey,fieldKey);
                Db.HashSet(setKey,fieldKey,"OK");
				action();
			}
			finally
			{
				Db.HashDelete(setKey,fieldKey);
			}
		}

		public void Execute(string name, Action<object> action, object obj)
		{
			WaitforRedial.WaitforRedialFinish();
            string setKey = GetSetKey();
            string fieldKey = GetFieldKey(name);

			try
			{
				//Db.SetAdd(setKey,fieldKey);
                Db.HashSet(setKey,fieldKey,"OK");
				action(obj);
			}
			finally
			{
				Db.HashDelete(setKey,fieldKey);
			}
		}

		public T Execute<T>(string name, Func<object, T> func, object obj)
		{
			WaitforRedial.WaitforRedialFinish();
            string setKey = GetSetKey();
            string fieldKey = GetFieldKey(name);

			try
			{
				//Db.SetAdd(setKey,fieldKey);
                Db.HashSet(setKey,fieldKey,"OK");
				return func(obj);
			}
			finally
			{
				Db.HashDelete(setKey,fieldKey);
			}
		}

		public T Execute<T>(string name, Func<T> func)
		{
			WaitforRedial.WaitforRedialFinish();
            string setKey = GetSetKey();
            string fieldKey = GetFieldKey(name);

			try
			{
				//Db.SetAdd(setKey,fieldKey);
                Db.HashSet(setKey,fieldKey,"OK");
				return func();
			}
			finally
			{
				Db.HashDelete(setKey,fieldKey);
			}
		}

		public void WaitAtomicAction()
		{
			// 等待数据库等操作完成
			while (true)
			{
				if (Db.SetLength(GetSetKey()) == 1)
				{
					break;
				}

				Thread.Sleep(1);
			}
		}
        
        public static string GetSetKey()
		{
			return HostName;
		}

		public static string GetFieldKey(string identity)
		{
			return SetPrefix + identity;
		}

		public IWaitforRedial WaitforRedial { get; }

		private static ConnectionMultiplexer Redis
		{
			get
			{
				if (_redis == null)
				{
					_redis = ConnectionMultiplexer.Connect(new ConfigurationOptions()
			        {
				        ServiceName = RedisHost,
				        ConnectTimeout = 5000,
				        KeepAlive = 8,
				        EndPoints =
				        {
					        { RedisHost, 6379 }
				        }
			        });
				}
				return _redis;
			}
		}
        
        private static IDatabase Db
		{
			get
			{
				if (_db == null)
				{                        
			        _db = Redis.GetDatabase(0);
				}
				return _db;
			}
		}

		internal RedisAtomicExecutor(IWaitforRedial waitforRedial)
		{
			if (waitforRedial == null)
			{
				throw new RedialException("IWaitforRedial can't be null.");
			}
			WaitforRedial = waitforRedial;
		}
	}
}