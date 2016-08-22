using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace DotnetSpider.Portal.Controllers
{
	public class DeployController : Controller
	{
		private static readonly string _updateCodesScriptPath = Path.Combine(AppContext.BaseDirectory, "updateCodes.script");

		public IActionResult Setting()
		{
			if (System.IO.File.Exists(_updateCodesScriptPath))
			{
				ViewBag.Script = System.IO.File.ReadAllText(_updateCodesScriptPath);
			}
			else
			{
				ViewBag.Script = null;
			}

			return View();
		}
	}
}
