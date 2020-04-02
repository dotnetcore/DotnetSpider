using Microsoft.AspNetCore.Mvc;

namespace DotnetSpider.Portal.Controllers
{
	public class SpiderContainerController : Controller
	{
		[HttpGet]
		public IActionResult Index()
		{
			return View();
		}
	}
}
