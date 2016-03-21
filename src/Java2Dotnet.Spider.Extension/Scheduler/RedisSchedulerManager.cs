using System;
using System.Collections.Generic;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Core.Utils;
using Java2Dotnet.Spider.Extension.Utils;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Java2Dotnet.Spider.Extension.Scheduler
{
	public class RedisSchedulerManager : ISchedulerManager, IDisposable
	{
		private readonly ConnectionMultiplexer _redis;
		private readonly IDatabase _db;

		public RedisSchedulerManager(string host, string password = null, int port = 6379)
		{
			_redis = ConnectionMultiplexer.Connect(new ConfigurationOptions()
			{
				ServiceName = host,
				Password = password,
				ConnectTimeout = 5000,
				KeepAlive = 8,
				AllowAdmin = true,
				EndPoints =
				{
					{ host, port }
				}
			});

			_db = _redis.GetDatabase(0);
		}

		public RedisSchedulerManager()
		{
			_redis = RedisProvider.GetProvider();
		}

		public IDictionary<string, double> GetTaskList(int startIndex, int count)
		{
			Dictionary<string, double> tmp = new Dictionary<string, double>();
			foreach (var entry in _db.SortedSetRangeByRank(RedisScheduler.TaskList, startIndex, startIndex + count))
			{
				tmp.Add(entry.ToString(), 0d);
			}
			return tmp;
		}

		public void RemoveTask(string taskIdentify)
		{
			_db.KeyDelete(GetQueueKey(taskIdentify));
			_db.KeyDelete(GetSetKey(taskIdentify));
			_db.HashDelete(RedisScheduler.TaskStatus, taskIdentify);
			_db.KeyDelete(RedisScheduler.ItemPrefix + taskIdentify);
			_db.KeyDelete(taskIdentify);
			_db.KeyDelete("locker-" + taskIdentify);
			_db.SortedSetRemove(RedisScheduler.TaskList, taskIdentify);
			_db.HashDelete("init-status", taskIdentify);
			_db.HashDelete("validate-status", taskIdentify);
			_db.KeyDelete("set-" + Encrypt.Md5Encrypt(taskIdentify));
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
			string json = _db.HashGet(RedisScheduler.TaskStatus, taskIdentify);
			if (!string.IsNullOrEmpty(json))
			{
				return JsonConvert.DeserializeObject<SpiderStatus>(json);
			}

			return new SpiderStatus();
		}

		public void ClearDb()
		{
			IServer server = _redis.GetServer(_redis.GetEndPoints()[0]);
			server.FlushDatabase();
		}

		public void Dispose()
		{
			_redis?.Close();
		}
	}
}
