using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using DotnetSpider.Portal.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace DotnetSpider.Portal.Controllers
{
	public class TaskStatusController : Controller
	{
		public IActionResult Dashboad(int? page = 1, int? pageSize = 15)
		{
			int no = page ?? 1;
			int size = pageSize ?? 15;
			using (MySqlConnection conn = new MySqlConnection(Startup.Configuration.GetSection("ConnectionStrings")["MySqlConnectString"]))
			{
				var totalCount = conn.Query<CountResult>("SELECT COUNT(*) AS Count FROM nlog.status").First();
				var totalPage = totalCount.Count / size + (totalCount.Count % size > 0 ? 1 : 0);
				ViewBag.DataUrl = $"/taskstatus/list/?page={no}&pageSize={size}";
				ViewBag.CurrentPage = no;
				ViewBag.TotalPage = (int)totalPage;
				return View();
			}
		}

		public IActionResult List(int? page = 1, int? pageSize = 15)
		{
			using (MySqlConnection conn = new MySqlConnection(Startup.Configuration.GetSection("ConnectionStrings")["MySqlConnectString"]))
			{
				int no = page ?? 1;
				int size = pageSize ?? 15;
				List<TaskStatus> list = GetItems(conn, no, size);
				return View(list);
			}
		}

		private List<TaskStatus> GetItems(IDbConnection conn, int page, int pageSize)
		{
			return conn.Query<TaskStatus>($"SELECT * FROM nlog.status ORDER BY id DESC LIMIT {(page - 1) * pageSize},{pageSize}").ToList();
		}
	}
}
