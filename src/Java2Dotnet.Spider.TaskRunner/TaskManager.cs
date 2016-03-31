using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Extension;
#if !NET_CORE
using log4net;
#else
using Java2Dotnet.Spider.JLog;
#endif
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Java2Dotnet.Spider.ScriptsConsole
{
	public class TaskManager
	{
		public const string TestTaskId = "15db56c44a6844ccaabc5caf735a6944";

#if !NET_CORE
		private readonly ILog _logger = LogManager.GetLogger(typeof(ScriptSpider));
#else
		private readonly ILog _logger = LogManager.GetLogger();
#endif

		private readonly ConnectionMultiplexer _redis;
		private IDatabase _db;

		public TaskManager()
		{
			_redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
			{
				ServiceName = "redis_primary",
				ConnectTimeout = 5000,
				KeepAlive = 8,
                		SyncTimeout=50000,
        			ResponseTimeout=50000,
        			Password = "#frAiI^MtFxh3Ks&swrnVyzAtRTq%w",
#if !RELEASE
				AllowAdmin = true,
#endif
				EndPoints =
				{
					{ "redis_primary", 6379 }
				}
			});
            			_redis.PreserveAsyncOrder = false;
				_db = _redis.GetDatabase(2);
		}

		public void TriggerTask(string hostName, string userId, string taskId)
		{
			var subscriber = _redis.GetSubscriber(hostName);
			subscriber.Publish(hostName, JsonConvert.SerializeObject(new TaskInfo { UserId = userId, TaskId = taskId }));
		}

		public void AddTask(string userId, string spiderScript)
		{
			string id = Guid.NewGuid().ToString();
			// todo: step 1 Map to user
			// step 2 Add to cache

			_db.HashSet(userId, id, spiderScript);
		}

		public void AddTestTask(string userId, string spiderScript)
		{
			// step 2 Add to cache
			_db.HashSet(userId, TestTaskId, spiderScript);
		}

		//public void Start()
		//{
		//	ManageSpiderNodes();
		//}

		//private void ManageSpiderNodes()
		//{
		//	ClearTimeoutNodes();
		//}

		//private void ClearTimeoutNodes()
		//{
		//	var nodes = _db.HashGetAll(SpiderRegistKey);
		//	foreach (var node in nodes)
		//	{
		//		NodeInfo nodeInfo = JsonConvert.DeserializeObject<NodeInfo>(node.Value);
		//		if ((DateTime.Now - nodeInfo.Timestamp).TotalSeconds > 30)
		//		{
		//			_db.HashDelete(SpiderRegistKey, node.Name);
		//			if (_spiderNodes.Contains(node.Name))
		//			{
		//				_spiderNodes.Remove(node.Name);
		//			}
		//		}
		//		else
		//		{
		//			if (!_spiderNodes.Contains(node.Name))
		//			{
		//				_spiderNodes.Add(node.Name);
		//			}
		//		}
		//	}
		//}
	}
}
