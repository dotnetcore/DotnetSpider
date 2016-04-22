using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Java2DotnetSpider.Portal.Controllers
{
	public class BaseController : ApiController
	{
		protected MongoClient GetMongoClinet()
		{
			var mongodbConnectString = ConfigurationManager.AppSettings["mongodb"];

			return new MongoClient(mongodbConnectString);
		}
	}
}
