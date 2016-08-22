using System;
using System.Collections.Generic;

namespace DotnetSpider.Portal.Models
{
	public class TaskStatus
	{
		public string TaskGroup { get; set; }
		public string UserId { get; set; }
		public string Identity { get; set; }
		public string Status { get; set; }

		public string StatusClass
		{
			get
			{
				switch (Status)
				{
					case "Running":
						{
							return "badge bg-green";
						}
					case "Exit":
						{
							return "badge bg-red";
						}
					case "Finished":
						{
							return "badge bg-light-blue";
						}
					case "Init":
						{
							return "badge bg-yellow";
						}
				}
				return "";
			}
		}
		public string Message { get; set; }
		public DateTime Logged { get; set; }
		public Int64 Id { get; set; }

		public static List<TaskStatus> Create(int num)
		{
			var list = new List<TaskStatus>();
			for (int i = 0; i < num; ++i)
			{
				list.Add(new TaskStatus
				{
					Id = i,
					TaskGroup = "YY",
					UserId = "86Research",
					Identity = "YY Channel " + i,
					Status = "Finished",
					Message = "Left: 0 Total: 100 Success: 100 Error: 0",
					Logged = DateTime.Now
				});
			}
			return list;
		}
	}
}
