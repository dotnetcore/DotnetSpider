using System.Threading;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Scheduler;
using Java2Dotnet.Spider.Core.Scheduler.Component;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Redial;
using Newtonsoft.Json;
using RedisSharp;

namespace Java2Dotnet.Spider.Extension.Scheduler
{
	/// <summary>
	/// Use Redis as url scheduler for distributed crawlers.
	/// </summary>
	public class RedisScheduler : DuplicateRemovedScheduler, IMonitorableScheduler, IDuplicateRemover
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
				Redis.SortedSetAdd(TaskList, spider.Identity, (long)DateTimeUtils.GetCurrentTimeStamp());
			});
		}

		public override void ResetDuplicateCheck(ISpider spider)
		{
			RedialManagerUtils.Execute("rds-reset", () =>
			{
				Redis.KeyDelete(GetSetKey(spider.Identity));
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

		public bool IsDuplicate(Request request, ISpider spider)
		{
			return SafeExecutor.Execute(30, () =>
			{
				string key = GetSetKey(spider.Identity);
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
				Redis.ListRightPush(GetQueueKey(spider.Identity), request.Identity);
				string field = request.Identity;
				string value = JsonConvert.SerializeObject(request);

				Redis.HashSet(ItemPrefix + spider.Identity, field, value);
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
				long size = Redis.ListLength(GetQueueKey(spider.Identity));
				return (int)size;
			});
		}

		public int GetTotalRequestsCount(ISpider spider)
		{
			return RedialManagerUtils.Execute("rds-gettotalcount", () =>
			{
				long size = Redis.SetLength(GetSetKey(spider.Identity));

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
				var value = Redis.ListRightPop(GetQueueKey(spider.Identity));
				if (value == null)
				{
					return null;
				}
				string field = value.ToString();
				string hashId = ItemPrefix + spider.Identity;

				string json = null;

				//redis 有可能取数据失败
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

				// 严格意义上说不会走到这里, 一定会有JSON数据,详情看Push方法
				// 是否应该设为1级？

				return null;
			});
		}
	}
}
