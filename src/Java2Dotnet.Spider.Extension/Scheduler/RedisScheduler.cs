using System.Threading;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Scheduler;
using Java2Dotnet.Spider.Core.Scheduler.Component;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Redial;
using Newtonsoft.Json;
using System.Collections.Generic;
using StackExchange.Redis;

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
		private string ErrorCountKey = "error-count-";
		private string ErrorRequestsKey = "error-requests-";
		private string SuccessCountKey = "success-count-";

		public IDatabase Db;
		public ConnectionMultiplexer Redis;

		public RedisScheduler(string host, string password = null, int port = 6379) : this(ConnectionMultiplexer.Connect(new ConfigurationOptions()
		{
			ServiceName = "DotnetSpider",
			Password = password,
			ConnectTimeout = 5000,
			KeepAlive = 8,
			EndPoints =
				{ host, port.ToString() }
		}))
		{
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

			RedialManagerUtils.Execute("rds-init", () =>
			{
				Db.SortedSetAdd(TaskList, spider.Identity, (long)DateTimeUtils.GetCurrentTimeStamp());
			});
		}

		public override void ResetDuplicateCheck()
		{
			RedialManagerUtils.Execute("rds-reset", () =>
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
			return RedialManagerUtils.Execute("rds-poll", () => DoPoll());
		}

		public int GetLeftRequestsCount()
		{
			return RedialManagerUtils.Execute("rds-getleftcount", () =>
			{
				long size = Db.ListLength(QueueKey);
				return (int)size;
			});
		}

		public int GetTotalRequestsCount()
		{
			return RedialManagerUtils.Execute("rds-gettotalcount", () =>
			{
				long size = Db.SetLength(SetKey);

				return (int)size;
			});
		}

		public override void Dispose()
		{
			Db.KeyDelete(SuccessCountKey);
			Db.KeyDelete(ErrorCountKey);
		}

		private Request DoPoll()
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
		}

		public override void Load(HashSet<Request> requests)
		{
			lock (this)
			{
				RedisValue[] identities = new RedisValue[requests.Count];
				HashEntry[] items = new HashEntry[requests.Count];
				int i = 0;
				foreach (var request in requests)
				{
					identities[i] = request.Identity;
					items[i] = new HashEntry(request.Identity, JsonConvert.SerializeObject(request));
					++i;
				}

				Db.SetAdd(SetKey, identities);
				Db.ListRightPush(QueueKey, identities);
				Db.HashSet(ItemKey, items);
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

		public void Clear()
		{
			Db.KeyDelete(QueueKey);
			Db.KeyDelete(SetKey);
			Db.KeyDelete(ItemKey);
			Db.KeyDelete(SuccessCountKey);
			Db.KeyDelete(ErrorCountKey);
			Db.KeyDelete(ErrorRequestsKey);
		}
	}
}
