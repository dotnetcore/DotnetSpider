using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Java2DotnetSpider.Portal.Controllers
{
	public class BaseController : Controller
	{
		protected MongoClient GetMongoClinet()
		{
			var mongodbConnectString = Startup.Configuration.GetSection("AppSetting").Get<string>("mongodb");

			return new MongoClient(mongodbConnectString);
		}
	}
}
