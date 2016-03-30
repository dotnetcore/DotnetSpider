using System;
using System.Dynamic;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.ScriptsConsole
{
	public class TaskInfo
	{
		public string UserId { get; set; }
		public string TaskId { get; set; }
		public string[] Arguments { get; set; }
		public DateTime Timestamp { get; set; }
		public Commands Command { get; set; }

		public enum Commands
		{
			Start,
			Stop,
			Kill
		}
	}
}
