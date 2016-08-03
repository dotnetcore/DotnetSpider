using System.Threading;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Scheduler;
using Java2Dotnet.Spider.Core.Scheduler.Component;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Redial;
using Newtonsoft.Json;
using System.Collections.Generic;
using StackExchange.Redis;
using System;
using System.Net;
using System.Linq;
using System.Runtime.InteropServices;

namespace Java2Dotnet.Spider.Extension.Scheduler
{
	/// <summary>
	/// Use Redis as url scheduler for distributed crawlers.
	/// </summary>
	public sealed class RedisScheduler : DuplicateRemovedScheduler, IMonitorableScheduler, IDuplicateRemover
	{
		public static string TaskList = "task";
		public static string TaskStatus = "task-status";
		private string QueueKey = "queue-";
		private string SetKey = "set-";
		private string ItemKey = "item-";
		private string ErrorCountKey = "error-record";
		private string SuccessCountKey = "success-record";
		private string IdentityMd5;

		public IDatabase Db;
		public ConnectionMultiplexer Redis;

		public RedisScheduler(string host, string password = null, int port = 6379)
		{
#if NET_CORE
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				// Lewis: This is a Workaround for .NET CORE can't use EndPoint to create Socket.
				host = Dns.GetHostAddressesAsync(host).Result.FirstOrDefault()?.ToString();
				if (string.IsNullOrEmpty(host))
				{
					throw new SpiderExceptoin("Can't resovle your host: " + host);
				}
			}
#endif

			Redis = ConnectionMultiplexer.Connect(new ConfigurationOptions()
			{
				ServiceName = "DotnetSpider",
				Password = password,
				ConnectTimeout = 8000,
				KeepAlive = 8,
				ConnectRetry = 20,
				SyncTimeout = 65530,
				ResponseTimeout = 65530,
				EndPoints =
				{ host, port.ToString() }
			});
			Db = Redis.GetDatabase(0);
		}

		public RedisScheduler(ConnectionMultiplexer redis) : this()
		{
			Redis = redis;
			Db = redis.GetDatabase(0);
		}

		private RedisScheduler()
		{
			DuplicateRemover = this;
		}

		public override void Init(ISpider spider)
		{
			base.Init(spider);

			var md5 = Encrypt.Md5Encrypt(spider.Identity);
			ItemKey += md5;
			SetKey += md5;
			QueueKey = md5;
			ErrorCountKey += md5;
			SuccessCountKey += md5;
			IdentityMd5 = md5;

			RedialManagerUtils.Execute("rds-in", () =>
			{
				Db.SortedSetAdd(TaskList, spider.Identity, (long)DateTimeUtils.GetCurrentTimeStamp());
			});
		}

		public override void ResetDuplicateCheck()
		{
			RedialManagerUtils.Execute("rds-rs", () =>
			{
				Db.KeyDelete(SetKey);
			});
		}

		public bool IsDuplicate(Request request)
		{
			return SafeExecutor.Execute(30, () =>
			{
				bool isDuplicate = Db.SetContains(SetKey, request.Identity);
				if (!isDuplicate)
				{
					Db.SetAdd(SetKey, request.Identity);
				}
				return isDuplicate;
			});
		}

		//[MethodImpl(MethodImplOptions.Synchronized)]
		protected override void PushWhenNoDuplicate(Request request)
		{
			SafeExecutor.Execute(30, () =>
			{
				Db.ListRightPush(QueueKey, request.Identity);
				string field = request.Identity;
				string value = JsonConvert.SerializeObject(request);

				Db.HashSet(ItemKey, field, value);
			});
		}

		//[MethodImpl(MethodImplOptions.Synchronized)]
		public override Request Poll()
		{
			return RedialManagerUtils.Execute("rds-pl", () =>
			{
				return SafeExecutor.Execute(30, () =>
				{
					var value = Db.ListRightPop(QueueKey);
					if (!value.HasValue)
					{
						return null;
					}
					string field = value.ToString();

					string json = Db.HashGet(ItemKey, field);

					if (!string.IsNullOrEmpty(json))
					{
						var result = JsonConvert.DeserializeObject<Request>(json);
						Db.HashDelete(ItemKey, field);
						return result;
					}
					return null;
				});
			});
		}

		public long GetLeftRequestsCount()
		{
			return RedialManagerUtils.Execute("rds-lc", () =>
			{
				return Db.ListLength(QueueKey);
			});
		}

		public long GetTotalRequestsCount()
		{
			return RedialManagerUtils.Execute("rds-tc", () =>
			{
				return Db.SetLength(SetKey);
			});
		}

		public long GetSuccessRequestsCount()
		{
			return RedialManagerUtils.Execute("rds-src", () =>
			{
				var result = Db.HashGet(SuccessCountKey, IdentityMd5);
				return result.HasValue ? (long)result : 0;
			});
		}

		public long GetErrorRequestsCount()
		{
			return RedialManagerUtils.Execute("rds-erc", () =>
			{
				var result = Db.HashGet(ErrorCountKey, IdentityMd5); ;
				return result.HasValue ? (long)result : 0;
			});
		}

		public void IncreaseSuccessCounter()
		{
			RedialManagerUtils.Execute("rds-isc", () =>
			{
				Db.HashIncrement(SuccessCountKey, IdentityMd5, 1);
			});
		}

		public void IncreaseErrorCounter()
		{
			RedialManagerUtils.Execute("rds-iec", () =>
			{
				Db.HashIncrement(ErrorCountKey, IdentityMd5, 1);
			});
		}

		public override void Dispose()
		{
			Db.KeyDelete(SuccessCountKey);
			Db.KeyDelete(ErrorCountKey);
		}

		public override void Load(HashSet<Request> requests)
		{
			lock (this)
			{
				int cacheSize = requests.Count > 10000 ? 10000 : requests.Count;
				RedisValue[] identities = new RedisValue[cacheSize];
				HashEntry[] items = new HashEntry[cacheSize];
				int i = 0;
				int j = requests.Count % 10000;
				int n = requests.Count / 10000;

				foreach (var request in requests)
				{
					identities[i] = request.Identity;
					items[i] = new HashEntry(request.Identity, JsonConvert.SerializeObject(request));
					++i;
					if (i == 10000)
					{
						--n;

						Db.SetAdd(SetKey, identities);
						Db.ListRightPush(QueueKey, identities);
						Db.HashSet(ItemKey, items);

						i = 0;
						if (n != 0)
						{
							identities = new RedisValue[10000];
							items = new HashEntry[10000];
						}
						else
						{
							identities = new RedisValue[j];
							items = new HashEntry[j];
						}
					}
				}

				if (i > 0)
				{
					Db.SetAdd(SetKey, identities);
					Db.ListRightPush(QueueKey, identities);
					Db.HashSet(ItemKey, items);
				}
			}
		}

		public override HashSet<Request> ToList()
		{
			HashSet<Request> requests = new HashSet<Request>();
			Request request;
			while ((request = Poll()) != null)
			{
				requests.Add(request);
			}
			return requests;
		}

		public override void Clear()
		{
			base.Clear();

			Db.KeyDelete(QueueKey);
			Db.KeyDelete(SetKey);
			Db.KeyDelete(ItemKey);
			Db.KeyDelete(SuccessCountKey);
			Db.KeyDelete(ErrorCountKey);
		}
	}
}
