using System;
using System.Net;
using System.Threading;
using DotnetSpider.Core.Redial.InternetDetector;
using DotnetSpider.Core.Redial.Redialer;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Core.Redial;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Redial
{
	public class RedisRedialExecutor : RedialExecutor
	{
		public static readonly string HostName = $"dotnetspider:nodes:{Env.HostName}";
		public static string RedialLockerKey = $"dotnetspider:redialLocker:{Env.HostName}";
		public static string RedialTimeKey = $"dotnetspider:redialLocker{Env.HostName}";

		public string ConnectString { get; }

		public RedisConnection RedisConnection { get; }

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

		public override void WaitAllNetworkRequestComplete()
		{
			RedisConnection.Database.HashSet(HostName, RedialLockerKey, DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
			while (true)
			{
				ClearTimeoutAction();
				var hashSet = RedisConnection.Database.HashGetAll(HostName);
				if (hashSet.Length == 1)
				{
					if (hashSet[0].Value.HasValue && hashSet[0].Name.ToString() == RedialLockerKey)
					{
						break;
					}
				}
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

		public override void LockRedial()
		{
			var key = $"{RedialLockerKey}:{HostName}";
			while (!RedisConnection.Database.LockTake(key, HostName, TimeSpan.FromSeconds(120)))
			{
				Thread.Sleep(50);
				continue;
			}
		}

		public override void ReleaseRedialLock()
		{
			var key = $"{RedialLockerKey}:{HostName}";
			RedisConnection.Database.LockRelease(key, HostName);
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

		public override bool IsRedialing()
		{
			return RedisConnection.Database.KeyExists(RedialLockerKey);
		}

		public override DateTime GetLastRedialTime()
		{
			var redialTime = RedisConnection.Database.StringGet(RedialTimeKey);
			if (redialTime.HasValue)
			{
				return DateTime.Parse(redialTime.ToString().Trim());
			}
			else
			{
				return new DateTime();
			}
		}

		public override void RecordLastRedialTime()
		{
			RedisConnection.Database.StringSet(RedialTimeKey, DateTime.Now.ToString());
		}
	}
}
