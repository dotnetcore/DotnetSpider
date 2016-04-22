using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Java2Dotnet.Spider.Portal.Models
{
	public class LogMessage
	{
		public string Type { get; set; }
		public string Time { get; set; }
		public string Message { get; set; }
		public string Machine { get; set; }
		public string UserId { get; set; }
		public string TaskId { get; set; }

		public LogInfo ToLogInfo()
		{
			return new LogInfo { Type = Type, Time = Time, Message = Message, Machine = Machine };
		}
	}

	public class LogInfo
	{
		public ObjectId _id { get; set; }
		public string Type { get; set; }
		public string Time { get; set; }
		public string Message { get; set; }
		public string Machine { get; set; }
	}
}