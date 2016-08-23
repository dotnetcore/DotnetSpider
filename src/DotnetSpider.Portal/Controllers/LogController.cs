using System.Collections.Generic;
using System.Linq;
using Dapper;
using DotnetSpider.Portal.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace DotnetSpider.Portal.Controllers
{
	public class LogController : Controller
	{
		public IActionResult List(string identity, int? page = 1, int? pageSize = 15)
		{
			using (MySqlConnection conn = new MySqlConnection(Startup.Configuration.GetSection("ConnectionStrings")["MySqlConnectString"]))
			{
				int no = page ?? 1;
				int size = pageSize ?? 15;

				List<LogInfo> list = conn.Query<LogInfo>($"SELECT * FROM dotnetspider.log where identity='{identity}' ORDER BY id DESC LIMIT {(no - 1) * size},{size}").ToList();
				var totalCount = conn.Query<CountResult>($"SELECT COUNT(*) AS Count FROM dotnetspider.log where identity='{identity}'").First();
				var totalPage = totalCount.Count / size + (totalCount.Count % size > 0 ? 1 : 0);
				ViewBag.CurrentPage = no;
				ViewBag.TotalPage = totalPage == 0 ? 1 : (int)totalPage;
				ViewBag.Identity = identity;
				return View(list);
			}
		}
	}
}
