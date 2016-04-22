using System;
using System.Text;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Configuration;
using Java2DotnetSpider.Portal.Entity;
using System.Net.Http;
using MongoDB.Driver;
using System.Net;
using MongoDB.Bson;

namespace Java2DotnetSpider.Portal.Controllers
{
	[Route("api/[controller]")]
	public class StatusController : BaseController
	{
		[HttpPost("upload")]
		public string Post([FromBody]StatusInfo info)
		{
			try
			{
				if (info == null || string.IsNullOrEmpty(info.UserId) || string.IsNullOrEmpty(info.TaskId) || string.IsNullOrEmpty(info.Name) || string.IsNullOrEmpty(info.Machine))
				{
					return "Error: UserId or TaskId or Name or Machine is null.";
				}

				var db = GetMongoClinet().GetDatabase(info.UserId);
				var collection = db.GetCollection<StatusInfo>("TASK_STATUS");

				var item = collection.Find(s => s.Name == info.Name && s.Machine == info.Machine).FirstOrDefault();

				if (item == null)
				{
					collection.InsertOne(info);
				}
				else
				{
					info._id = item._id;
					collection.ReplaceOne(s => s.Name == info.Name && s.Machine == info.Machine, info);
				}

				return "OK";
			}
			catch (Exception e)
			{
				return "ERROR";
			}
		}

		[HttpGet()]
		public string Get(string userid, string id)
		{
			try
			{
				var db = GetMongoClinet().GetDatabase(userid);
				var collection = db.GetCollection<StatusInfo>("TASK_STATUS");
				StringBuilder builder = new StringBuilder("<ul>");
				foreach (var element in collection.Find(e => e._id == new MongoDB.Bson.ObjectId(id)).ToList())
				{
					var pagePerSecondg = element.Message.PagePerSecond.ToString();
					builder.Append("<li>");
					builder.Append($"<h class='bold'>Status:</h><h class='{GetStatusClass(element.Message.Status)}'>").Append(element.Message.Status).Append("</h> ")
						.Append("<h class='bold'>Identify:</h><h class='value'>").Append(element.Name).Append("</h> ")
						.Append("<h class='bold'>Machine:</h></h><h class='value'>").Append(element.Machine).Append("</h> ")
						.Append("<h class='bold'>Left:</h><h class='value'>").Append(element.Message.LeftPageCount).Append("</h> ")
						.Append("<h class='bold'>Total:</h><h class='value'>").Append(element.Message.TotalPageCount).Append("</h> ")
						.Append("<h class='bold'>Thread:</h><h class='value'>").Append(element.Message.ThreadCount).Append("</h> ")
						.Append("<h class='bold'>Success:</h><h class='value'>").Append(element.Message.SuccessPageCount).Append("</h> ")
						.Append("<h class='bold'>Error:</h><h class='value'>").Append(element.Message.ErrorPageCount).Append("</h> ")
						.Append("<h class='bold'>Speed:</h><h class='value'>").Append(pagePerSecondg.Length > 4 ? pagePerSecondg.Substring(0, 3) : pagePerSecondg).Append("</h> ")
						.Append("<h class='bold'>Start:</h><h class='value'>").Append(element.Message.StartTime.ToString("yyyyMMdd hh:mm")).Append("</h> ")
						.Append("<h class='bold'>End:</h><h class='value'>").Append(element.Message.EndTime.ToString("yyyyMMdd hh:mm")).Append("</h> ")
						.Append("</li>");
				}
				builder.Append("</ul>");
				return builder.ToString();
			}
			catch (Exception e)
			{
				return null;
			}
		}

		[HttpGet("{userid}/{taskid}")]
		public string Get(string userid, string taskid, int page, int offset)
		{
			try
			{
				var db = GetMongoClinet().GetDatabase(userid);
				var collection = db.GetCollection<StatusInfo>("TASK_STATUS");
				//return collection.Find(e => e.TaskId == taskid).SortByDescending(e => e._id).Skip(offset * (page - 1)).Limit(offset).ToJson();

				StringBuilder builder = new StringBuilder("<ul>");
				foreach (var element in collection.Find(e => e.TaskId == taskid).SortByDescending(e => e._id).Skip(offset * (page - 1)).Limit(offset).ToList())
				{
					var pagePerSecondg = element.Message.PagePerSecond.ToString();
					var editUrl = WebUtility.UrlEncode($"userid={userid}&name={element.Name}&id={element._id}");
					builder.Append("<li>");
					builder.Append($"<h class='bold'>Status:</h><h class='{GetStatusClass(element.Message.Status)}'>").Append(element.Message.Status).Append("</h> ")
						.Append("<h class='bold'>Identify:</h><h class='value'>").Append(element.Name).Append("</h> ")
						.Append("<h class='bold'>Machine:</h></h><h class='value'>").Append(element.Machine).Append("</h> ")
						.Append("<h class='bold'>Left:</h><h class='value'>").Append(element.Message.LeftPageCount).Append("</h> ")
						.Append("<h class='bold'>Total:</h><h class='value'>").Append(element.Message.TotalPageCount).Append("</h> ")
						.Append("<h class='bold'>Thread:</h><h class='value'>").Append(element.Message.ThreadCount).Append("</h> ")
						.Append("<h class='bold'>Success:</h><h class='value'>").Append(element.Message.SuccessPageCount).Append("</h> ")
						.Append("<h class='bold'>Error:</h><h class='value'>").Append(element.Message.ErrorPageCount).Append("</h> ")
						.Append("<h class='bold'>Speed:</h><h class='value'>").Append(pagePerSecondg.Length > 4 ? pagePerSecondg.Substring(0, 3) : pagePerSecondg).Append("</h> ")
						.Append("<h class='bold'>Start:</h><h class='value'>").Append(element.Message.StartTime.ToString("yyyyMMdd hh:mm")).Append("</h> ")
						.Append("<h class='bold'>End:</h><h class='value'>").Append(element.Message.EndTime.ToString("yyyyMMdd hh:mm")).Append("</h> ")
						.Append(GetControlHtml(element))
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

		private string GetControlHtml(StatusInfo result)
		{
			if (result.Message.Status == "Running")
			{
				return $"<a href=\"#\" class='action' onclick=\"stop('{result.UserId}','{result.Name}')\">Stop</a>&nbsp;&nbsp;<a href=\"#\" class='action' onclick=\"exit('{result.UserId}','{result.Name}')\">Exit</a>&nbsp;&nbsp;<a href=\"#\" class='action' onclick=\"del('{result.UserId}','{result._id}')\">Delete</a> <br><br>";
			}
			else if (result.Message.Status == "Init")
			{
				return $"<a href=\"#\" class='action'  onclick=\"stop('{result.UserId}','{result.Name}')\">Stop</a>&nbsp;&nbsp;<a href=\"#\"  class='action' onclick=\"exit('{result.UserId}','{result.Name}')\">Exit</a>&nbsp;&nbsp;<a href=\"#\" class='action' onclick=\"del('{result.UserId}','{result._id}')\">Delete</a> <br><br>";
			}
			else if (result.Message.Status == "Stopped")
			{
				return $"<a href=\"#\" class='action'  onclick=\"start('{result.UserId}','{result.Name}')\">Start</a>&nbsp;&nbsp;<a href=\"#\"  class='action' onclick=\"exit('{result.UserId}','{result.Name}')\">Exit</a>&nbsp;&nbsp;<a href=\"#\" class='action' onclick=\"del('{result.UserId}','{result._id}')\">Delete</a> <br><br>";
			}
			else
			{
				return $"<a href=\"#\" class='action' onclick=\"del('{result.UserId}','{result._id}')\">Delete</a> <br><br>";
			}
		}

		private object GetStatusClass(string status)
		{
			switch (status)
			{
				case "Running":
					{
						return "running";
					}
				case "Stopped":
					{
						return "stopped";
					}
				case "Finished":
					{
						return "finished";
					}
				case "Init":
					{
						return "init";
					}
			}
			return "unknow";
		}
	}
}
