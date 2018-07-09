using System;

namespace DotnetSpider.Core.Infrastructure
{
	public class LogInfo
	{
		public string Identity;
		public string NodeId;
		public DateTimeOffset Logged = DateTime.UtcNow;
		public string Level;
		public string Message;
		public string Exception;
	}
}