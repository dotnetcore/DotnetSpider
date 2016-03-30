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
#endif
using Newtonsoft.Json;
using StackExchange.Redis;

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
		private readonly ConnectionMultiplexer _redis;
		private readonly IDatabase _db;
		private readonly string _hostName;

		private readonly Dictionary<string, ScriptSpider> _spiderCache = new Dictionary<string, ScriptSpider>();

		public SpiderNode()
		{
			_hostName = Dns.GetHostName();
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
				if (!IsConnected)
				{
					_logger.Warn($" SpiderNode: {_hostName} is disconnected OR there is another same name node exits.");
					return;
				}
				try
				{
					var taskInfo = JsonConvert.DeserializeObject<TaskInfo>(message);

					string value = _db.HashGet(taskInfo.UserId, taskInfo.TaskId);

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
						_db.HashSet(SpiderRegistKey, _hostName, JsonConvert.SerializeObject(new NodeInfo()
						{
							Name = Dns.GetHostName(),
#if !NET_CORE
							CpuLoad = (int)systemInfo.CpuLoad,
							TotalMemory = (int)(systemInfo.PhysicalMemory / (1024 * 1024)),
							FreeMemory = (int)((systemInfo.PhysicalMemory - systemInfo.MemoryAvailable) / (1024 * 1024)),
#endif
							Timestamp = DateTime.Now
						}));
						IsConnected = true;
					}

					Thread.Sleep(3000);
				}
				// ReSharper disable once FunctionNeverReturns
			});
		}
	}
}
