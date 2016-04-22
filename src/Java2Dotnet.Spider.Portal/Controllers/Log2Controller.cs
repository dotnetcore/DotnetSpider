using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Java2Dotnet.Spider.Portal.Controllers
{
	[RoutePrefix("log")]
	public class Log2Controller : Controller
	{
		public ActionResult Index()
		{
			ViewBag.Title = "Log Manager";

			return View();
		}
	}
}
