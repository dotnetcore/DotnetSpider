using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Core;
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
using StackExchange.Redis;
using MySql.Data.MySqlClient;
using System.Data;
 
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
		public readonly ConnectionMultiplexer _redis;
		public readonly IDatabase _db;
		private readonly string _hostName;

		private readonly Dictionary<string, ScriptSpider> _spiderCache = new Dictionary<string, ScriptSpider>();

		public SpiderNode()
		{
			_hostName = Dns.GetHostName();
			_redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
			{
				ServiceName = "redis_primary",
				ConnectTimeout = 5000,
				KeepAlive = 8,
				Password = "#frAiI^MtFxh3Ks&swrnVyzAtRTq%w",
#if !RELEASE
				AllowAdmin = true,
#endif
				EndPoints =
				{
					{ "redis_primary", 6379 }
				}
			});
			_db = _redis.GetDatabase(2);
		}

		public void Run()
		{
			RegistNode();

			Subscrib();

			Thread.Sleep(4000);
		}

		private void Subscrib()
		{
			var subscriber = _redis.GetSubscriber(_hostName);
			subscriber.Subscribe($"{_hostName}", (channel, message) =>
			{
				Task.Factory.StartNew(()=>{
					if (!IsConnected)
					{
						_logger.Warn($" SpiderNode: {_hostName} is disconnected OR there is another same name node exits.");
						return;
					}
					try
					{
						var taskInfo = JsonConvert.DeserializeObject<TaskInfo>(message);
                        string value=null;
                        using (var conn = new MySqlConnection("Database='mysql';Data Source=office.86research.cn;User ID=root;Password=1qazZAQ!;Port=3306"))
			            {
                           	conn.Open();
				            var command = conn.CreateCommand();
                            string sql=$"SELECT `script` FROM `java2dotnet.spider`.`spider_scripts` where user_id='{taskInfo.UserId}' and task_id='{taskInfo.TaskId}'";
                            command.CommandText = sql;
				            command.CommandType = CommandType.Text;
                            var reader = command.ExecuteReader();
                        
				            if(reader.Read())
				            {				        	 
						        value = reader.GetString(0);
				            }

				            reader.Close();
                        }
                        
                        if(string.IsNullOrEmpty(value)){
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
						_logger.Error($"Execute spider: {message} failed. Details:" + e);
					}
				});
			});
		}

		private void RegistNode()
		{
#if !NET_CORE
			SystemInfo systemInfo = new SystemInfo();
#endif

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
#if !NET_CORE
                NodeInfo nodeInfo= new NodeInfo()
						{
							Name = Dns.GetHostName(),

							CpuLoad = (int)systemInfo.CpuLoad,
							TotalMemory = (int)(systemInfo.PhysicalMemory / (1024 * 1024)),
							FreeMemory = (int)((systemInfo.PhysicalMemory - systemInfo.MemoryAvailable) / (1024 * 1024)),
                             OS="Windows",
							Timestamp = DateTime.Now
						};
#else
                        NodeInfo nodeInfo=new NodeInfo();
                        nodeInfo.Name=Dns.GetHostName();                        
                        nodeInfo.Timestamp=DateTime.Now;
                        
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				var info= LinuxSystemInfo.GetSystemInfo();
                nodeInfo.CpuLoad=(int)(info.LoadAvg*100);
                nodeInfo.TotalMemory=info.TotalMemory;
                nodeInfo.FreeMemory=info.FreeMemory;
                nodeInfo.OS="Linux";
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
 //todo:
  nodeInfo.OS="OSX";
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
 //todo:
   nodeInfo.OS="Windows";
			}
#endif
                        
						_db.HashSet(SpiderRegistKey, _hostName, JsonConvert.SerializeObject(nodeInfo));
						IsConnected = true;
					}

					Thread.Sleep(3000);
				}
				// ReSharper disable once FunctionNeverReturns
			});
		}
	}
}
