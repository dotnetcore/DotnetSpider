using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Portal.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace DotnetSpider.Portal.Controllers
{
	public class DashboadController : Controller
	{
		public IActionResult TaskStatus(int id = 1)
		{
			using (MySqlConnection conn = new MySqlConnection(Startup.Configuration.GetSection("ConnectionStrings")["MySqlConnectString"]))
			{
				var pageOption = new MoPagerOption
				{
					CurrentPage = id,
					PageSize = 15,
					RouteUrl = "/dashboad/taskstatus"
				};
				List<Models.TaskStatus> result = conn.Query<Models.TaskStatus>($"SELECT * FROM nlog.status LIMIT {(id - 1) * pageOption.PageSize},{pageOption.PageSize}").ToList();

				var totalCount = conn.Query<CountResult>("SELECT COUNT(*) AS Count FROM nlog.status").First();
				pageOption.Total = totalCount.Count;
				//分页参数
				ViewBag.PagerOption = pageOption;
				//var result = _taskStatuses.Skip((pageOption.CurrentPage - 1) * pageOption.PageSize).Take(pageOption.PageSize).ToList();
				return View(result);
			}

		}
	}
}
