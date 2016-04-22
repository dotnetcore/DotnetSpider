using System;
using System.Configuration;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using System.Net.Http;
using Java2DotnetSpider.Portal.Entity;

namespace Java2DotnetSpider.Portal.Controllers
{
	[Route("api/[controller]")]
	public class LogController : BaseController
	{
		[HttpPost("upload")]
		public string Post([FromBody]LogMessage log)
		{
			try
			{
				var db = GetMongoClinet().GetDatabase(log.UserId);
				var collection = db.GetCollection<LogInfo>(log.TaskId);
				collection.InsertOne(log.ToLogInfo());
				return "Ok";
			}
			catch (Exception e)
			{
				return "Error";
			}
		}

		[HttpGet("tasks/{userid}")]
		public string Get(string userid)
		{
			try
			{
				var db = GetMongoClinet().GetDatabase(userid);

				var tasks = db.ListCollections().ToList();

				tasks.RemoveAll(t => (t["name"] == "system.indexes") || (t["name"] == "TASK_STATUS"));

				StringBuilder builder = new StringBuilder();
				builder.Append("<br/>");
				builder.Append("<br/>");
				foreach (var task in tasks)
				{
					builder.Append($"<a class=\"taska\" href=\"#\" onclick=\"searchLogs('{userid}','{task["name"]}');\">{task["name"]}</a>");
					builder.Append("<br/>");
				}
				builder.Append("<br/>");
				builder.Append("<br/>");
				return builder.ToString();
			}
			catch (Exception e)
			{
				return null;
			}
		}

		[HttpGet("{userid}/{taskid}")]
		public string Get(string userid, string taskid, int page, int offset = 18)
		{
			try
			{
				var db = GetMongoClinet().GetDatabase(userid);
				var collection = db.GetCollection<LogInfo>(taskid);
				StringBuilder builder = new StringBuilder("<ul>");

				foreach (var element in collection.Find(l => l._id != new MongoDB.Bson.ObjectId()).SortByDescending(e => e._id).Skip(offset * (page - 1)).Limit(offset).ToList())
				{
					builder.Append("<li class='log'>");
					builder.Append($"<h class='{GetLogTypeClass(element.Type)}'> ")
						.Append(element.Type).Append("</h> <h class='value'> ")
						.Append(element.Machine).Append("</h> <h class='time'> ")
						.Append(element.Time.Substring(0, 16)).Append("</h> ")
						.Append(element.Message).Append("")
						.Append("</li>");
				}
				builder.Append("</ul>");
				return builder.ToString();
			}
			catch (Exception e)
			{
				return "Error";
			}
		}

		private object GetLogTypeClass(string type)
		{
			switch (type)
			{
				case "WARNING":
					{
						return "warning";
					}
				case "ERROR":
					{
						return "error";
					}
				case "INFO":
					{
						return "info";
					}
			}
			return "info";
		}
	}
}
