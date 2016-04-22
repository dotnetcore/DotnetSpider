using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Java2DotnetSpider.Portal.Controllers
{
	[Route("api/[controller]")]
	public class TaskController : BaseController
	{
		[HttpGet("{userid}")]
		public string Get(string userid)
		{
			try
			{
				var db = GetMongoClinet().GetDatabase(userid);

				var tasks = db.ListCollections().ToList();

				tasks.RemoveAll(t => (t["name"] == "system.indexes") || (t["name"] == "TASK_STATUS"));
 
				return tasks.ToJson();
			}
			catch (Exception e)
			{
				return null;
			}
		}
	}
}
