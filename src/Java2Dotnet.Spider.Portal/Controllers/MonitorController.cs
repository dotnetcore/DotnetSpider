
using Java2Dotnet.Spider.Portal.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Java2DotnetSpider.Portal.Controllers
{
	[RoutePrefix("api/monitor")]
	public class MonitorController : BaseController
	{
		static MonitorController()
		{
			ConnectionMultiplexer context = ConnectionMultiplexer.Connect(new ConfigurationOptions
			{
				ServiceName = ConfigurationManager.AppSettings["redisHost"],
				Password = ConfigurationManager.AppSettings["redisPassword"],
				EndPoints = {
							{ ConfigurationManager.AppSettings["redisHost"], 6379 }
						}
			});
			db = context.GetDatabase(4);
		}

		private static IDatabase db;

		public HttpResponseMessage Get(string userid, string id)
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
					string html;
					if (result.Message.Status == "Running")
					{
						html = $"<br><a href=\"#\" class='action' onclick=\"stop()\">Stop</a>&nbsp;&nbsp;<a href=\"#\"  class='action' onclick=\"exit()\">Exit</a>&nbsp;&nbsp;<a href=\"#\" class='action' onclick=\"del()\">Delete</a> <br><br>";
					}
					else if (result.Message.Status == "Init")
					{
						html = $"<br><a href=\"#\" class='action'  onclick=\"stop()\">Stop</a>&nbsp;&nbsp;<a href=\"#\"  class='action' onclick=\"exit()\">Exit</a>&nbsp;&nbsp;<a href=\"#\" class='action' onclick=\"del()\">Delete</a> <br><br>";
					}
					else if (result.Message.Status == "Stopped")
					{
						html = $"<br><a href=\"#\" class='action'  onclick=\"start()\">Start</a>&nbsp;&nbsp;<a href=\"#\"  class='action' onclick=\"exit()\">Exit</a>&nbsp;&nbsp;<a href=\"#\" class='action' onclick=\"del()\">Delete</a> <br><br>";
					}
					else
					{
						html = null;
					}
					return new HttpResponseMessage { Content = new StringContent(html, Encoding.UTF8, "application/json") };
				}

			}
			catch (Exception e)
			{
				return null;
			}
		}

		public string Get(string op, string userid, string name)
		{
			try
			{
				if (op == "stop")
				{
					db.Publish($"{userid}-{name}", "stop");
					return "ok";
				}
				if (op == "start")
				{
					db.Publish($"{userid}-{name}", "start");
					return "ok";
				}
				if (op == "exit")
				{
					db.Publish($"{userid}-{name}", "exit");
					return "ok";
				}
				else if (op == "delete")
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
