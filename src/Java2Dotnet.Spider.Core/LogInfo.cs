using System;
using System.Net;

namespace Java2Dotnet.Spider.Core
{
	public class LogInfo
	{
		public static string _machine = Dns.GetHostName();

		public static string Create(string message, ITask task)
		{
			return $"[{DateTime.Now}] [{_machine}][{task.UserId}][{task.TaskGroup}] {message}";
		}
	}
}
