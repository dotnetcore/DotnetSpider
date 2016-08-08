using System.Net;

namespace DotnetSpider.Core
{
	public class LogInfo
	{
		public static string Machine = Dns.GetHostName();

		public static string Create(string message, ITask task)
		{
			return $"[{task.UserId}][{task.TaskGroup}] {message}";
		}
	}
}
