using System;
using System.Net;
using System.Threading;
using DotnetSpider.Extension.Redial.InternetDetector;
using DotnetSpider.Extension.Redial.Redialer;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Extension.Redial
{
	public class RedisRedialExecutor : RedialExecutor
	{
		public static string HostName => $"dotnetspider:nodes:{Dns.GetHostName()}";

		public const string Locker = "dotnetspider:redialLocker";

		public string ConnectString { get; private set; }
		public RedisConnection RedisConnection { get; private set; }

		public RedisRedialExecutor(string connectString, IRedialer redialer, IInternetDetector validater) : base(redialer, validater)
		{
			ConnectString = connectString;

			RedisConnection = Cache.Instance.Get(ConnectString);
			if (RedisConnection == null)
			{
				RedisConnection = new RedisConnection(ConnectString);
				Cache.Instance.Set(ConnectString, RedisConnection);
			}
		}

		public override void WaitAll()
		{
			RedisConnection.Database.HashSet(HostName, Locker, DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
			while (true)
			{
				ClearTimeoutAction();
				var hashSet = RedisConnection.Database.HashGetAll(HostName);
				if (hashSet.Length == 1)
				{
					if (hashSet[0].Value.HasValue && hashSet[0].Name.ToString() == Locker)
					{
						break;
					}
				}
				Thread.Sleep(50);
			}
		}

		public override void WaitRedialExit()
		{
			var locker = RedisConnection.Database.HashGet(HostName, Locker);
			while (locker.HasValue)
			{
				ClearTimeoutRedialLocker();
				locker = RedisConnection.Database.HashGet(HostName, Locker);
				Thread.Sleep(50);
			}
		}

		public override string CreateActionIdentity(string name)
		{
			string identity = name + "_" + Guid.NewGuid().ToString("N");
			RedisConnection.Database.HashSet(HostName, identity, DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
			return identity;
		}

		public override void DeleteActionIdentity(string identity)
		{
			RedisConnection.Database.HashDelete(HostName, identity);
		}

		public override bool CheckIsRedialing()
		{
			lock (Lock)
			{
				var locker = RedisConnection.Database.HashGet(HostName, Locker);
				if (!locker.HasValue)
				{
					//RedisConnection.Database.HashSet(HostName, Locker, DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
					return false;
				}
				return true;
			}
		}

		public override void ReleaseRedialLocker()
		{
			var locker = RedisConnection.Database.HashGet(HostName, Locker);
			while (locker.HasValue)
			{
				Console.WriteLine("Try releasing redis redial-locker");
				RedisConnection.Database.HashDelete(HostName, Locker);
				locker = RedisConnection.Database.HashGet(HostName, Locker);
				Thread.Sleep(50);
			}
		}

		private void ClearTimeoutAction()
		{
			foreach (var entry in RedisConnection.Database.HashGetAll(HostName))
			{
				string key = entry.Name;
				string value = entry.Value;

				DateTime dt = DateTime.Parse(value);
				var minutes = (DateTime.Now - dt).TotalMinutes;

				if (minutes > 5)
				{
					RedisConnection.Database.HashDelete(HostName, key);
				}
			}
		}

		private void ClearTimeoutRedialLocker()
		{
			var result = RedisConnection.Database.HashGet(HostName, Locker);
			if (result.HasValue)
			{
				var value = DateTime.Parse(result.ToString());
				var minutes = (DateTime.Now - value).TotalMinutes;

				if (minutes > 5)
				{
					RedisConnection.Database.HashDelete(HostName, Locker);
				}
			}
		}
	}
}
