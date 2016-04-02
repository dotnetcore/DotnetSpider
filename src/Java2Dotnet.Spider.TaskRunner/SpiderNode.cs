using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Extension;
using Java2Dotnet.Spider.Extension.Configuration;
using Java2Dotnet.Spider.Extension.Configuration.Json;
#if !NET_CORE
using log4net;
#else
using Java2Dotnet.Spider.JLog;
using System.Runtime.InteropServices;
#endif
using Newtonsoft.Json;
//using StackExchange.Redis;
using MySql.Data.MySqlClient;
using System.Data;
using RedisSharp;

namespace Java2Dotnet.Spider.ScriptsConsole
{
	public class SpiderNode
	{
#if !NET_CORE
		private readonly ILog _logger = LogManager.GetLogger(typeof(ScriptSpider));
#else
		private readonly ILog _logger = LogManager.GetLogger();
#endif
		private const string SpiderRegistKey = "spider_nodes";
		public bool IsConnected { get; private set; }
		public readonly RedisServer _redisClient;

		//private readonly Dictionary<string, ScriptSpider> _spiderCache = new Dictionary<string, ScriptSpider>();

		public SpiderNode()
		{
			//			Redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
			//			{
			//				ServiceName = "redis_primary",
			//				ConnectTimeout = 5000,
			//				KeepAlive = 8,
			//				Password = "#frAiI^MtFxh3Ks&swrnVyzAtRTq%w",
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
			//Db = Redis.GetDatabase(2);
		}

		public void Run()
		{
			RegistNode();

			Subscrib();

			Thread.Sleep(4000);
		}

		private void Subscrib()
		{
			var redis = new RedisServer("ooodata.com", 6379,"#frAiI^MtFxh3Ks&swrnVyzAtRTq%w");
			redis.Db = 2;
			//var subscriber = Redis.GetSubscriber(SystemInfo.HostName);
			redis.Subscribe($"{SystemInfo.HostName}", (chanel, commandInfo) =>
			{
				Task.Factory.StartNew(() =>
				{
					if (!IsConnected)
					{
						_logger.Warn($" SpiderNode: {SystemInfo.HostName} is disconnected OR there is another same name node exits.");
						return;
					}
					try
					{
						var taskInfo = JsonConvert.DeserializeObject<TaskInfo>(commandInfo);
						string value = null;
						using (var conn = new MySqlConnection("Database='mysql';Data Source=office.86research.cn;User ID=root;Password=1qazZAQ!;Port=3306"))
						{
							conn.Open();
							var command = conn.CreateCommand();
							string sql = $"SELECT `script` FROM `java2dotnet.spider`.`spider_scripts` where user_id='{taskInfo.UserId}' and task_id='{taskInfo.TaskId}'";
							command.CommandText = sql;
							command.CommandType = CommandType.Text;
							var reader = command.ExecuteReader();

							if (reader.Read())
							{
								value = reader.GetString(0);
							}

							reader.Close();
						}

						if (string.IsNullOrEmpty(value))
						{
							_logger.Warn($"USERID: {taskInfo.UserId} TASKID: {taskInfo.TaskId} Script is NULL.");
							return;
						}

						string json = Macros.Replace(value);

						JsonSpiderContext spiderContext = JsonConvert.DeserializeObject<JsonSpiderContext>(json);

						List<string> errorMessages;
						if (SpiderContextValidation.Validate(spiderContext, out errorMessages))
						{
							ScriptSpider spider = new ScriptSpider(spiderContext.ToRuntimeContext());
							spider.Run(taskInfo.Arguments);
						}
						else
						{
							foreach (var errorMessage in errorMessages)
							{
								Console.WriteLine(errorMessage);
							}
						}
					}
					catch (Exception e)
					{
						_logger.Error($"Execute spider: {commandInfo} failed. Details:" + e);
					}
				});
			});
		}

		private void RegistNode()
		{
			Task.Factory.StartNew(() =>
			{
				while (true)
				{
					//var result = _db.HashGet(SpiderRegistKey, _hostName);
					//if (result.HasValue)
					//{
					//	var nodeInfo = JsonConvert.DeserializeObject<NodeInfo>(result.ToString());
					//	if ((DateTime.Now - nodeInfo.Timestamp).TotalSeconds < 30)
					//	{
					//		IsConnected = false;
					//		_logger.Warn($"HostName: {_hostName} already exist.");
					//	}
					//}
					//else
					{
						_redisClient.HashSet(SpiderRegistKey, SystemInfo.HostName, JsonConvert.SerializeObject(SystemInfo.GetSystemInfo()));
						IsConnected = true;
					}

					Thread.Sleep(3000);
				}
				// ReSharper disable once FunctionNeverReturns
			});
		}
	}
}
