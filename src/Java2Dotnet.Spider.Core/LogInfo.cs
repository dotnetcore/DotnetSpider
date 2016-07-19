using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Java2Dotnet.Spider.Core
{
	public class LogInfo
	{
		public static string _machine = Dns.GetHostName();
		public string Time { get; set; } = DateTime.Now.ToString();
		public string Message { get; set; } = "";
		public string Machine { get; } = _machine;
		public string TaskGroup { get; set; } = "";
		public string UserId { get; set; } = "DotnetSpider";

		public override string ToString()
		{
			return $"[{Time}] [{Machine}][{UserId}][{TaskGroup}] {Message}";
		}
	}
}
