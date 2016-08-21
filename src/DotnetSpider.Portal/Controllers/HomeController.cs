using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.Portal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using TaskStatus = DotnetSpider.Portal.Models.TaskStatus;

namespace DotnetSpider.Portal.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult About()
		{
			ViewData["Message"] = "Your application description page.";

			return View();
		}

		public IActionResult Contact()
		{
			ViewData["Message"] = "Your contact page.";

			return View();
		}

		public IActionResult Error()
		{
			return View();
		}

		public IActionResult Log(string id)
		{
			return View();
		}
	}
}
