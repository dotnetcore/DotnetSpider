using System;
using System.Net;
using System.Threading;
using DotnetSpider.Redial.InternetDetector;
using DotnetSpider.Redial.Redialer;
using StackExchange.Redis;

namespace DotnetSpider.Redial
{
	public class RedisRedialExecutor : BaseRedialExecutor
	{
		public IDatabase Db { get; }
		public static string HostName { get; set; } = Dns.GetHostName();
		public const string Locker = "redial-locker";

		public RedisRedialExecutor(IDatabase db, IRedialer redialer,   IInternetDetector validater) : base(redialer,  validater)
		{
			Db = db;
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
	}
}
