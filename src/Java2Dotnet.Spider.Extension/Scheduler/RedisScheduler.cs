using System.Threading;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Scheduler;
using Java2Dotnet.Spider.Core.Scheduler.Component;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Redial;
using Newtonsoft.Json;
using System.Collections.Generic;
using RedisSharp;
using System;

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
				Redis.SortedSetAdd(TaskList, spider.Identity, (long)DateTimeUtils.GetCurrentTimeStamp());
			});
		}

		public override void ResetDuplicateCheck()
		{
			RedialManagerUtils.Execute("rds-rs", () =>
			{
				Redis.KeyDelete(SetKey);
			});
		}

		public bool IsDuplicate(Request request)
		{
			return SafeExecutor.Execute(30, () =>
			{
				bool isDuplicate = Redis.SetContains(SetKey, request.Identity);
				if (!isDuplicate)
				{
					Redis.SetAdd(SetKey, request.Identity);
				}
				return isDuplicate;
			});
		}

		//[MethodImpl(MethodImplOptions.Synchronized)]
		protected override void PushWhenNoDuplicate(Request request)
		{
			SafeExecutor.Execute(30, () =>
			{
				Redis.ListRightPush(QueueKey, request.Identity);
				string field = request.Identity;
				string value = JsonConvert.SerializeObject(request);

				Redis.HashSet(ItemKey, field, value);
			});
		}

		//[MethodImpl(MethodImplOptions.Synchronized)]
		public override Request Poll()
		{
			return RedialManagerUtils.Execute("rds-pl", () =>
			{
				return SafeExecutor.Execute(30, () =>
				{
					var value = Redis.ListRightPop(QueueKey);
					if (string.IsNullOrEmpty(value))
					{
						return null;
					}
					string field = value.ToString();

					string json = Redis.HashGet(ItemKey, field);

					if (!string.IsNullOrEmpty(json))
					{
						var result = JsonConvert.DeserializeObject<Request>(json);
						Redis.HashDelete(ItemKey, field);
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
				return Redis.ListLength(QueueKey);
			});
		}

		public long GetTotalRequestsCount()
		{
			return RedialManagerUtils.Execute("rds-tc", () =>
			{
				return Redis.SetLength(SetKey);
			});
		}

		public long GetSuccessRequestsCount()
		{
			return RedialManagerUtils.Execute("rds-src", () =>
			{
				var result = Redis.HashGet(SuccessCountKey, IdentityMd5);
				return !string.IsNullOrEmpty(result) ? long.Parse(result) : 0;
			});
		}

		public long GetErrorRequestsCount()
		{
			return RedialManagerUtils.Execute("rds-erc", () =>
			{
				var result = Redis.HashGet(ErrorCountKey, IdentityMd5); ;
				return !string.IsNullOrEmpty(result) ? long.Parse(result) : 0;
			});
		}

		public void IncreaseSuccessCounter()
		{
			RedialManagerUtils.Execute("rds-isc", () =>
			{
				var value = Redis.HashGet(SuccessCountKey, IdentityMd5);
				if (!string.IsNullOrEmpty(value))
				{
					Redis.HashSet(SuccessCountKey, IdentityMd5, (long.Parse(value) + 1).ToString());
				}
			});
		}

		public void IncreaseErrorCounter()
		{
			RedialManagerUtils.Execute("rds-iec", () =>
			{
				var value = Redis.HashGet(ErrorCountKey, IdentityMd5);
				if (!string.IsNullOrEmpty(value))
				{
					Redis.HashSet(ErrorCountKey, IdentityMd5, (long.Parse(value) + 1).ToString());
				}
			});
		}

		public override void Dispose()
		{
			Redis.KeyDelete(SuccessCountKey);
			Redis.KeyDelete(ErrorCountKey);
		}

		public string GetSetKey(string identity)
		{
			return SetKey + Encrypt.Md5Encrypt(identity);
		}

		public string GetQueueKey(string identity)
		{
			return QueueKey + Encrypt.Md5Encrypt(identity);
		}

		public string GetItemKey(string identity)
		{
			return ItemKey + Encrypt.Md5Encrypt(identity);
		}

		public override void Load(HashSet<Request> requests)
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

				Redis.SetAddManay(GetSetKey(IdentityMd5), identities);

				Redis.ListRightPushMany(GetQueueKey(IdentityMd5), identities);

				Redis.HashSetMany(GetItemKey(IdentityMd5), jsonDic);
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

			Redis.KeyDelete(QueueKey);
			Redis.KeyDelete(SetKey);
			Redis.KeyDelete(ItemKey);
			Redis.KeyDelete(SuccessCountKey);
			Redis.KeyDelete(ErrorCountKey);
		}
	}
}
