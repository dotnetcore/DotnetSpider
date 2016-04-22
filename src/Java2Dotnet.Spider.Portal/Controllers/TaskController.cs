using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Java2DotnetSpider.Portal.Controllers
{
	[RoutePrefix("api/task")]
	public class TaskController : BaseController
	{
		[HttpGet()]
		public HttpResponseMessage Get(string userid)
		{
			try
			{
				var db = GetMongoClinet().GetDatabase(userid);

				var tasks = db.ListCollections().ToList();

				tasks.RemoveAll(t => (t["name"] == "system.indexes") || (t["name"] == "TASK_STATUS"));

				//return tasks.ToJson();
				return new HttpResponseMessage { Content = new StringContent(tasks.ToJson(), Encoding.UTF8, "application/json") };
			}
			catch (Exception e)
			{
				return null;
			}
		}
	}
}
