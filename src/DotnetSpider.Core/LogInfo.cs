using System;
using System.Net;

namespace DotnetSpider.Core
{
	public class LogInfo
	{
		public static string _machine = Dns.GetHostName();

		public static string Create(string message, ITask task)
		{
			return $"[{task.UserId}][{task.TaskGroup}] {message}";
		}
	}
}
