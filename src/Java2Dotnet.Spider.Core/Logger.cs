
using Java2Dotnet.Spider.Ioc;
using Java2Dotnet.Spider.Log;
using System;
using System.Collections.Generic;
using System.Text;

namespace Java2Dotnet.Spider.Core
{
	public class Logger : ILogService
	{
		private static List<ILogService> Services = ServiceProvider.Get<ILogService>();
		private string SpiderName { get; set; }
		private string UserId { get; set; }
		private string TaskGroup { get; set; }

		public Logger(string spiderName, string userId, string taskGroup)
		{
			SpiderName = spiderName;
			UserId = userId;
			TaskGroup = taskGroup;
		}

		public void Warn(dynamic message, Exception e)
		{
			var log = new LogInfo { UserId = UserId, TaskGroup = TaskGroup, Message = message };
			foreach (var service in Services)
			{
				service.Warn(log, e);
			}
		}

		public void Warn(dynamic message)
		{
			var log = new LogInfo { UserId = UserId, TaskGroup = TaskGroup, Message = message };
			foreach (var service in Services)
			{
				service.Warn(log);
			}
		}

		public void Info(dynamic message, Exception e)
		{
			var log = new LogInfo { UserId = UserId, TaskGroup = TaskGroup, Message = message };
			foreach (var service in Services)
			{
				service.Info(log, e);
			}
		}

		public void Info(dynamic message)
		{
			var log = new LogInfo { UserId = UserId, TaskGroup = TaskGroup, Message = message };
			foreach (var service in Services)
			{
				service.Info(log);
			}
		}

		public void Error(dynamic message, Exception e)
		{
			var log = new LogInfo { UserId = UserId, TaskGroup = TaskGroup, Message = message };
			foreach (var service in Services)
			{
				service.Error(log, e);
			}
		}

		public void Error(dynamic message)
		{
			var log = new LogInfo { UserId = UserId, TaskGroup = TaskGroup, Message = message };
			foreach (var service in Services)
			{
				service.Error(log);
			}
		}

		public void Dispose()
		{
			foreach (var d in Services)
			{
				d.Dispose();
			}
		}
	}
}
