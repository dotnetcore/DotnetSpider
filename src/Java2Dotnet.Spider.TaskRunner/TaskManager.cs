using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Extension;
using log4net;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Java2Dotnet.Spider.ScriptsConsole
{
	public class TaskManager
	{
		public const string TestTaskId = "1b18f77c-99da-4a28-a5f3-dc23725b1618";

#if !NET_CORE
		private readonly ILog _logger = LogManager.GetLogger(typeof(ScriptSpider));
#else
		private readonly ILog _logger = LogManager.GetLogger();
#endif

		private readonly ConnectionMultiplexer _redis;
		private readonly IDatabase _db;

		public TaskManager()
		{
			_redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
			{
				//ServiceName = "118.126.11.168",
				ServiceName = "localhost",
				ConnectTimeout = 5000,
				//Password = "#frAiI^MtFxh3Ks&swrnVyzAtRTq%w",
				KeepAlive = 8,
#if !RELEASE
				AllowAdmin = true,
#endif
				EndPoints =
				{
					//{ "118.126.11.168", 6379 }
					{ "localhost", 6379 }
				}
			});
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
