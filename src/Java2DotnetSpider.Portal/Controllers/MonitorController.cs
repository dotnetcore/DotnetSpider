using Java2DotnetSpider.Portal.Entity;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Java2DotnetSpider.Portal.Controllers
{
	[Route("api/[controller]")]
	public class MonitorController : BaseController
	{
		private static ConnectionMultiplexer context = ConnectionMultiplexer.Connect(new ConfigurationOptions
		{
			ServiceName = Startup.Configuration.GetSection("AppSetting").Get<string>("redisHost"),
			Password = Startup.Configuration.GetSection("AppSetting").Get<string>("redisPassword"),
			EndPoints = {
							{ Startup.Configuration.GetSection("AppSetting").Get<string>("redisHost"), 6379 }
						}
		});

		private static IDatabase db = context.GetDatabase(4);

		[HttpGet("{userid}/{id}")]
		public string Get(string userid, string id)
		{
			try
			{
				var db = GetMongoClinet().GetDatabase(userid);
				var collection = db.GetCollection<StatusInfo>("TASK_STATUS");
				var result = collection.Find(e => e._id == new MongoDB.Bson.ObjectId(id)).FirstOrDefault();
				if (result == null)
				{
					return null;
				}
				else
				{
					if (result.Message.Status == "Running")
					{
						return $"<br><a href=\"#\" class='action' onclick=\"stop()\">Stop</a>&nbsp;&nbsp;<a href=\"#\"  class='action' onclick=\"exit()\">Exit</a>&nbsp;&nbsp;<a href=\"#\" class='action' onclick=\"del()\">Delete</a> <br><br>";
					}
					else if (result.Message.Status == "Init")
					{
						return $"<br><a href=\"#\" class='action'  onclick=\"stop()\">Stop</a>&nbsp;&nbsp;<a href=\"#\"  class='action' onclick=\"exit()\">Exit</a>&nbsp;&nbsp;<a href=\"#\" class='action' onclick=\"del()\">Delete</a> <br><br>";
					}
					else if (result.Message.Status == "Stopped")
					{
						return $"<br><a href=\"#\" class='action'  onclick=\"start()\">Start</a>&nbsp;&nbsp;<a href=\"#\"  class='action' onclick=\"exit()\">Exit</a>&nbsp;&nbsp;<a href=\"#\" class='action' onclick=\"del()\">Delete</a> <br><br>";
					}
					else
					{
						return null;
					}
				}

			}
			catch (Exception e)
			{
				return null;
			}
		}

		[HttpGet("{operate}")]
		public string Operate(string operate, string userid, string name)
		{
			try
			{
				if (operate == "stop")
				{
					db.Publish($"{userid}-{name}", "stop");
					return "ok";
				}
				if (operate == "start")
				{
					db.Publish($"{userid}-{name}", "start");
					return "ok";
				}
				if (operate == "exit")
				{
					db.Publish($"{userid}-{name}", "exit");
					return "ok";
				}
				else if (operate == "delete")
				{
					var db = GetMongoClinet().GetDatabase(userid);
					var collection = db.GetCollection<StatusInfo>("TASK_STATUS");
					var result = collection.DeleteOne(e => e._id == new ObjectId(name));
					return (result.DeletedCount > 0) ? "ok" : "error";
				}
				return "error";
			}
			catch
			{
				return "error";
			}
		}
	}
}
