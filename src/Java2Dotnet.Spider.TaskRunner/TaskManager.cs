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
using System.Data.Common;
using MySql.Data.MySqlClient;
using System.Data;
using RedisSharp;

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

		private readonly RedisServer _redisClient;

		public TaskManager()
		{
			//			_redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
			//			{
			//				ServiceName = "redis_primary",
			//				ConnectTimeout = 5000,
			//				KeepAlive = 8,
			//                		SyncTimeout=50000,
			//        			ResponseTimeout=50000,
			//        			Password = "#frAiI^MtFxh3Ks&swrnVyzAtRTq%w",
			//#if !RELEASE
			//				AllowAdmin = true,
			//#endif
			//				EndPoints =
			//				{
			//					{ "redis_primary", 6379 }
			//				}
			//			});

			_redisClient = new RedisServer("ooodata.com", 6379, "#frAiI^MtFxh3Ks&swrnVyzAtRTq%w");
			_redisClient.Db = 2;
		}

		public void TriggerTask(string hostName, string userId, string taskId)
		{
			_redisClient.Publish(hostName, JsonConvert.SerializeObject(new TaskInfo { UserId = userId, TaskId = taskId }));
		}

		public void AddTask(string userId, string taskId, string spiderScript)
		{
			//string id = Guid.NewGuid().ToString();
			// todo: step 1 Map to user
			// step 2 Add to cache

/*
			using (var conn = new MySqlConnection("Database='mysql';Data Source=office.86research.cn;User ID=root;Password=1qazZAQ!;Port=3306"))
			{
				List<Dictionary<string, object>> datas = new List<Dictionary<string, object>>();
				string sql = "INSERT IGNORE INTO `java2dotnet.spider`.`spider_scripts` (`user_id`,`task_id`,`script`) values(@user_id,@task_id,@script)";
				conn.Open();
				var command = conn.CreateCommand();
				command.CommandText = sql;
				command.CommandType = CommandType.Text;

				List<DbParameter> parameters = new List<DbParameter>();

				var parameter1 = new MySqlParameter();
				parameter1.ParameterName = "@user_id";
				parameter1.Value = userId;
				parameter1.DbType = DbType.String;
				parameters.Add(parameter1);

				var parameter2 = new MySqlParameter();
				parameter2.ParameterName = "@task_id";
				parameter2.Value = taskId;
				parameter2.DbType = DbType.String;
				parameters.Add(parameter2);

				var parameter3 = new MySqlParameter();
				parameter3.ParameterName = "@script";
				parameter3.Value = spiderScript;
				parameter3.DbType = DbType.String;
				parameters.Add(parameter3);

				command.Parameters.AddRange(parameters.ToArray());
				command.ExecuteNonQuery();
			}
            */
_redisClient.HashSet(userId,taskId,spiderScript);
		}

		public void AddTestTask(string userId, string spiderScript)
		{
			// step 2 Add to cache
			AddTask(userId, TestTaskId, spiderScript);
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
