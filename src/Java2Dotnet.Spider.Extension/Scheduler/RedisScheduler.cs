using System.Threading;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Scheduler;
using Java2Dotnet.Spider.Core.Scheduler.Component;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Redial;
using Newtonsoft.Json;
using RedisSharp;
using System.Collections.Generic;

namespace Java2Dotnet.Spider.Extension.Scheduler
{
	/// <summary>
	/// Use Redis as url scheduler for distributed crawlers.
	/// </summary>
	public sealed class RedisScheduler : DuplicateRemovedScheduler, IMonitorableScheduler, IDuplicateRemover
	{
		public static readonly string QueuePrefix = "queue-";
		public static readonly string TaskStatus = "task-status";
		public static readonly string SetPrefix = "set-";
		public static readonly string TaskList = "task";
		public static readonly string ItemPrefix = "item-";

		public RedisServer Redis { get; }

		public RedisScheduler(string host, string password = null, int port = 6379) : this(new RedisServer(host, port, password))
		{
		}

		public RedisScheduler(RedisServer redis) : this()
		{
			Redis = redis;
			Redis.Db = 0;
		}

		private RedisScheduler()
		{
			DuplicateRemover = this;
		}

		public override void Init(ISpider spider)
		{
			RedialManagerUtils.Execute("rds-init", () =>
			{
				Redis.SortedSetAdd(TaskList, GetIdentity(spider), (long)DateTimeUtils.GetCurrentTimeStamp());
			});
		}

		private static string GetIdentity(ISpider spider)
		{
			return $"{spider.UserId}-{spider.Identity}";
		}

		public override void ResetDuplicateCheck(ISpider spider)
		{
			RedialManagerUtils.Execute("rds-reset", () =>
			{
				Redis.KeyDelete(GetSetKey(GetIdentity(spider)));
			});
		}

		public static string GetSetKey(string identity)
		{
			return SetPrefix + Encrypt.Md5Encrypt(identity);
		}

		public static string GetQueueKey(string identity)
		{
			return QueuePrefix + Encrypt.Md5Encrypt(identity);
		}

		public static string GetItemKey(string identity)
		{
			return ItemPrefix + Encrypt.Md5Encrypt(identity);
		}

		public bool IsDuplicate(Request request, ISpider spider)
		{
			return SafeExecutor.Execute(30, () =>
			{
				string key = GetSetKey(GetIdentity(spider));
				bool isDuplicate = Redis.SetContains(key, request.Identity);
				if (!isDuplicate)
				{
					Redis.SetAdd(key, request.Identity);
				}
				return isDuplicate;
			});
		}

		//[MethodImpl(MethodImplOptions.Synchronized)]
		protected override void PushWhenNoDuplicate(Request request, ISpider spider)
		{
			SafeExecutor.Execute(30, () =>
			{
				Redis.ListRightPush(GetQueueKey(GetIdentity(spider)), request.Identity);
				string field = request.Identity;
				string value = JsonConvert.SerializeObject(request);

				Redis.HashSet(GetItemKey(GetIdentity(spider)), field, value);
			});
		}

		//[MethodImpl(MethodImplOptions.Synchronized)]
		public override Request Poll(ISpider spider)
		{
			return RedialManagerUtils.Execute("rds-poll", () => DoPoll(spider));
		}

		public int GetLeftRequestsCount(ISpider spider)
		{
			return RedialManagerUtils.Execute("rds-getleftcount", () =>
			{
				long size = Redis.ListLength(GetQueueKey(GetIdentity(spider)));
				return (int)size;
			});
		}

		public int GetTotalRequestsCount(ISpider spider)
		{
			return RedialManagerUtils.Execute("rds-gettotalcount", () =>
			{
				long size = Redis.SetLength(GetSetKey(GetIdentity(spider)));

				return (int)size;
			});
		}

		public override void Dispose()
		{
			Redis?.Dispose();
		}

		private Request DoPoll(ISpider spider)
		{
			return SafeExecutor.Execute(30, () =>
			{
				var value = Redis.ListRightPop(GetQueueKey(GetIdentity(spider)));
				if (value == null)
				{
					return null;
				}
				string field = value.ToString();
				string hashId = GetItemKey(GetIdentity(spider));

				string json = null;

				for (int i = 0; i < 10 && string.IsNullOrEmpty(json = Redis.HashGet(hashId, field)); ++i)
				{
					Thread.Sleep(150);
				}

				if (!string.IsNullOrEmpty(json))
				{
					var result = JsonConvert.DeserializeObject<Request>(json);
					Redis.HashDelete(hashId, field);
					return result;
				}

				return null;
			});
		}

		public override void Load(HashSet<Request> requests, ISpider spider)
		{
			lock (this)
			{
				List<string> identities = new List<string>();
				Dictionary<string, string> jsonDic = new Dictionary<string, string>();
				foreach (var request in requests)
				{
					identities.Add(request.Identity);
					jsonDic.Add(request.Identity, JsonConvert.SerializeObject(request));
				}

				Redis.SetAddManay(GetSetKey(GetIdentity(spider)), identities);

				Redis.ListRightPushMany(GetQueueKey(GetIdentity(spider)), identities);

				Redis.HashSetMany(GetItemKey(GetIdentity(spider)), jsonDic);
			}
		}

		public override HashSet<Request> ToList(ISpider spider)
		{
			HashSet<Request> requests = new HashSet<Request>();
			Request request;
			while ((request = Poll(spider)) != null)
			{
				requests.Add(request);
			}
			return requests;
		}

		public void Clear(ISpider spider)
		{
			Redis.KeyDelete(GetQueueKey(GetIdentity(spider)));
			Redis.KeyDelete(GetSetKey(GetIdentity(spider)));
			Redis.KeyDelete(GetItemKey(GetIdentity(spider)));
		}
	}
}
