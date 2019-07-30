using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DotnetSpider.DownloadAgentRegisterCenter.Entity;
using DotnetSpider.Portal.Models;
using DotnetSpider.Statistics.Entity;
using Microsoft.AspNetCore.Http.Internal;
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
			ViewData["Agent"] = await _dbContext.Set<DownloaderAgent>().CountAsync();
			ViewData["OnlineAgent"] = await _dbContext.Set<DownloaderAgent>().CountAsync(x => x.IsActive());
			ViewData["Repository"] = await _dbContext.DockerRepositories.CountAsync();
			ViewData["Spider"] = await _dbContext.Spiders.CountAsync();
			ViewData["RunningSpider"] = await _dbContext.Set<SpiderStatistics>()
				.CountAsync(x => (DateTime.Now - x.LastModificationTime).TotalSeconds < 60);
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
		}
	}
}