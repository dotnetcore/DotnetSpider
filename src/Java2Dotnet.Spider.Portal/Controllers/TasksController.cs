using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Java2Dotnet.Spider.Portal.Controllers
{
	public class TasksController : Controller
	{
		public ActionResult Index()
		{
			ViewBag.Title = "Task Manager";

			return View();
		}
	}
}
