using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace DotnetSpider.Portal.Controllers
{
	public class DeployController : Controller
	{
		private static readonly string UpdateCodesScriptPath = Path.Combine(AppContext.BaseDirectory, "updateCodes.script");

		public IActionResult Setting()
		{
			if (System.IO.File.Exists(UpdateCodesScriptPath))
			{
				ViewBag.Script = System.IO.File.ReadAllText(UpdateCodesScriptPath);
			}
			else
			{
				ViewBag.Script = null;
			}

			return View();
		}

		public IActionResult Source()
		{
			return View();
		}
	}
}
