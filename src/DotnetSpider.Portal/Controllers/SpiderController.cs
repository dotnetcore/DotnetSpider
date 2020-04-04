using Microsoft.AspNetCore.Mvc;

namespace DotnetSpider.Portal.Controllers
{
	[Route("spiders")]
	public class SpiderController
		: Controller
	{
		[HttpGet]
		public IActionResult Index()
		{
			return View();
		}

		[HttpGet("{id}/histories")]
		public IActionResult History()
		{
			return View();
		}
	}
}
