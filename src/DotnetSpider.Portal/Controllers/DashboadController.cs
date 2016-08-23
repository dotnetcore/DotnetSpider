using System;
using System.Collections.Generic;
using DotnetSpider.Portal.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Dapper;
using System.Linq;

namespace DotnetSpider.Portal.Controllers
{
	public class DashboadController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Task()
		{
			using (MySqlConnection conn = new MySqlConnection(Startup.Configuration.GetSection("ConnectionStrings")["MySqlConnectString"]))
			{
				List<TaskStatus> list = conn.Query<TaskStatus>($"SELECT * FROM dotnetspider.status WHERE `logged`>='{DateTime.Now.ToString("yyyy-MM-dd")}'").ToList();
				ViewBag.TotalCount = list.Count;
				ViewBag.ExceptionCount = list.Count(t => t.Status != TaskStatus.StatusCode.Finished && DateTime.Now - t.Logged > new TimeSpan(0, 5, 0));
				ViewBag.ExceptionPercent = list.Count == 0 ? 0 : (int)(ViewBag.ExceptionCount / (float)ViewBag.TotalCount * 100);
				ViewBag.FinishedCount = list.Count(t => t.Status == TaskStatus.StatusCode.Finished);
				ViewBag.FinishedPercent = list.Count == 0 ? 0 : (int)(ViewBag.FinishedCount / (float)ViewBag.TotalCount * 100);
				ViewBag.RunningCount = list.Count - ViewBag.ExceptionCount - ViewBag.FinishedCount;
				ViewBag.RunningPercent = list.Count == 0 ? 0 : (int)(ViewBag.RunningCount / (float)ViewBag.TotalCount * 100);
				return View();
			}
		}

		public IActionResult Log()
		{
			using (MySqlConnection conn = new MySqlConnection(Startup.Configuration.GetSection("ConnectionStrings")["MySqlConnectString"]))
			{
				ViewBag.LogCount = conn.Query<CountResult>("SELECT COUNT(*) AS Count FROM dotnetspider.log;").First().Count / 10000 + "万";
				return View();
			}
		}
	}
}
