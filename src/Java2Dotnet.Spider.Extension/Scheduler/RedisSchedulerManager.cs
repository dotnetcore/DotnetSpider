using System;
using System.Collections.Generic;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Extension.Utils;
using Newtonsoft.Json;
using RedisSharp;

namespace Java2Dotnet.Spider.Extension.Scheduler
{
	public class RedisSchedulerManager : ISchedulerManager, IDisposable
	{
		public readonly RedisServer Redis;

		public RedisSchedulerManager(string host, string password = null, int port = 6379)
		{
			Redis = new RedisServer(host, port, password);
			Redis.Db = 0;
		}

		public RedisSchedulerManager()
		{
			Redis = RedisProvider.GetProvider();
		}

		public IDictionary<string, double> GetTaskList(int startIndex, int count)
		{
			Dictionary<string, double> tmp = new Dictionary<string, double>();
			foreach (var entry in Redis.SortedSetRangeByRank(RedisScheduler.TaskList, startIndex, startIndex + count))
			{
				tmp.Add(entry, 0d);
			}
			return tmp;
		}

		public void RemoveTask(string taskIdentify)
		{
			Redis.KeyDelete(GetQueueKey(taskIdentify));
			Redis.KeyDelete(GetSetKey(taskIdentify));
			Redis.HashDelete(RedisScheduler.TaskStatus, taskIdentify);
			Redis.KeyDelete(RedisScheduler.ItemPrefix + taskIdentify);
			Redis.KeyDelete(taskIdentify);
			Redis.KeyDelete("locker-" + taskIdentify);
			Redis.SortedSetRemove(RedisScheduler.TaskList, taskIdentify);
			Redis.HashDelete("init-status", taskIdentify);
			Redis.HashDelete("validate-status", taskIdentify);
			Redis.KeyDelete("set-" + Encrypt.Md5Encrypt(taskIdentify));
		}

		private string GetSetKey(string identify)
		{
			return RedisScheduler.SetPrefix + Encrypt.Md5Encrypt(identify);
		}

		private string GetQueueKey(string identify)
		{
			return RedisScheduler.QueuePrefix + Encrypt.Md5Encrypt(identify);
		}

		public SpiderStatus GetTaskStatus(string taskIdentify)
		{
			string json = Redis.HashGet(RedisScheduler.TaskStatus, taskIdentify);
			if (!string.IsNullOrEmpty(json))
			{
				return JsonConvert.DeserializeObject<SpiderStatus>(json);
			}

			return new SpiderStatus();
		}

		public void ClearDb()
		{
			Redis.FlushDb();
		}

		public void Dispose()
		{
			Redis?.Dispose();
		}
	}
}
