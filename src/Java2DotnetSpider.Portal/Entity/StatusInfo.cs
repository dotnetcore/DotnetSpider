using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Java2DotnetSpider.Portal.Entity
{
	public class StatusInfo
	{
		public ObjectId _id { get; set; }
		public string UserId { get; set; }
		public string TaskId { get; set; }
		public string Name { get; set; }
		public string Machine { get; set; }
		public StatusMessage Message { get; set; }
	}

	public class StatusMessage
	{
		public string Status { get; set; }
		public int AliveThreadCount { get; set; }
		public int ThreadCount { get; set; }
		public long TotalPageCount { get; set; }
		public long LeftPageCount { get; set; }
		public long SuccessPageCount { get; set; }
		public long ErrorPageCount { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public double PagePerSecond { get; set; }
	}
}