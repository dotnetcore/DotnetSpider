using System.Collections.Generic;
using System.Linq;
using Dapper;
using DotnetSpider.Portal.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace DotnetSpider.Portal.Controllers
{
	public class TaskStatusController : Controller
	{
		public IActionResult List(int? page = 1, int? pageSize = 15)
		{
			using (MySqlConnection conn = new MySqlConnection(Startup.Configuration.GetSection("ConnectionStrings")["MySqlConnectString"]))
			{
				int no = page ?? 1;
				int size = pageSize ?? 15;

				List<TaskStatus> list = conn.Query<TaskStatus>($"SELECT * FROM nlog.status LIMIT {(no - 1) * size},{size}").ToList();
				var totalCount = conn.Query<CountResult>("SELECT COUNT(*) AS Count FROM nlog.status").First();
				var totalPage = totalCount.Count / size + (totalCount.Count % size > 0 ? 1 : 0);
				ViewBag.CurrentPage = no;
				ViewBag.TotalPage = (int)totalPage;
				return View(list);
			}
		}
	}
}
