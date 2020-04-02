using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.AgentRegister.Store;
using DotnetSpider.Portal.Data;
using DotnetSpider.Portal.Models;
using DotnetSpider.Statistics.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetSpider.Portal.Controllers
{
	public class HomeController : Controller
	{
		private readonly PortalDbContext _dbContext;

		public HomeController(PortalDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<IActionResult> Index()
		{
			ViewData["Agent"] = await _dbContext.Set<AgentInfo>().CountAsync();
			var activeTime = DateTimeOffset.Now.AddSeconds(-60);
			ViewData["OnlineAgent"] =
				await (_dbContext.Set<AgentInfo>().CountAsync(x => x.LastModificationTime > activeTime));
			ViewData["Repository"] = await _dbContext.DockerRepositories.CountAsync();
			ViewData["Spider"] = await _dbContext.Spiders.CountAsync();
			ViewData["RunningSpider"] = await _dbContext.Set<SpiderStatistics>()
				.CountAsync(x => x.LastModificationTime > activeTime);
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
		}
	}
}
