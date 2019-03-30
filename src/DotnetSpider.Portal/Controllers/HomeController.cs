using System;
using System.Diagnostics;
using System.Linq;
using DotnetSpider.Portal.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetSpider.Portal.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Cookie()
        {
            return Content(string.Join($"; {Environment.NewLine}",
                HttpContext.Request.Cookies.Select(x => $"{x.Key}={x.Value}")));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}