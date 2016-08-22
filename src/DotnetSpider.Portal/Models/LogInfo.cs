using System;

namespace DotnetSpider.Portal.Models
{
	public class LogInfo
	{
		public string TaskGroup { get; set; }
		public string UserId { get; set; }
		public string Identity { get; set; }
		public string Level { get; set; }

		public string LevelClass
		{
			get
			{
				switch (Level)
				{
					case "Warn":
						{
							return "badge bg-yellow";
						}
					case "Fatal":
					case "Error":
						{
							return "badge bg-red";
						}
					case "Info":
						{
							return "badge bg-green";
						}
					case "Trace":
						{
							return "badge bg-gray";
						}
					case "Debug":
						{
							return "btn bg-purple";
						}
				}
				return "";
			}
		}
		public string Message { get; set; }
		public string CallSite { get; set; }		
		public DateTime Logged { get; set; }
		public Int64 Id { get; set; }
	}
}
