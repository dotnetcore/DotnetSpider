using Java2Dotnet.Spider.Ioc;
using System;
using System.Collections.Generic;

namespace Java2Dotnet.Spider.Log
{
	public class LogService : ILogService, IService
	{
		private List<ILogService> Services { get; set; } = new List<ILogService>();
		private string SpiderName { get; set; }
		private string UserId { get; set; }
		private string TaskGroup { get; set; }

		public LogService(params ILogService[] logServices)
		{
			foreach (var logService in logServices)
			{
				if ((logService as LogService) != null)
				{
					throw new Exception("Can't add LogService to LogService instanse.");
				}
				Services.Add(logService);
			}
		}

		public void AddService(ILogService service)
		{
			if ((service as LogService) != null)
			{
				throw new Exception("Can't add LogService to LogService instanse.");
			}
			Services.Add(service);
		}

		public void Warn(string message, Exception e)
		{
			foreach (var service in Services)
			{
				service.Warn(message, e);
			}
		}

		public void Warn(string message)
		{
			foreach (var service in Services)
			{
				service.Warn(message);
			}
		}

		public void Info(string message, Exception e)
		{
			foreach (var service in Services)
			{
				service.Info(message, e);
			}
		}

		public void Info(string message)
		{
			foreach (var service in Services)
			{
				service.Info(message);
			}
		}

		public void Error(string message, Exception e)
		{
			foreach (var service in Services)
			{
				service.Error(message, e);
			}
		}

		public void Error(string message)
		{
			foreach (var service in Services)
			{
				service.Error(message);
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
