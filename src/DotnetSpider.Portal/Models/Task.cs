using System;

namespace DotnetSpider.Portal.Models
{
	public class Task
	{
		public int Id { get; set; }
		public string TaskName { get; set; }
		public string ExecuteArguments { get; set; }
		public int Nodes { get; set; } = 1;
		public string Cron { get; set; }
		public DateTime CDate { get; set; }
		public DateTime LastUpdate { get; set; }
	}
}
