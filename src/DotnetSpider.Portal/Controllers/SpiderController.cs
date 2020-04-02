using Microsoft.AspNetCore.Mvc;

namespace DotnetSpider.Portal.Controllers
{
	public class SpiderController
		: Controller
	{
		[HttpGet]
		public IActionResult Index()
		{
			return View();
		}
	}
}
