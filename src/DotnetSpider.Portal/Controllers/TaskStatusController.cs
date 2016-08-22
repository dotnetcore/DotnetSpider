using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using Dapper;
using DotnetSpider.Portal.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using StackExchange.Redis;

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

		public string Stop(string identity)
		{
			var host = Startup.Configuration.GetSection("Redis")["Host"];
			var password = Startup.Configuration.GetSection("Redis")["Password"];
			int port;
			if (!int.TryParse(Startup.Configuration.GetSection("Redis")["Port"], out port) || port <= 0)
			{
				return "REDIS Port is incorrect.";
			}
			if (!string.IsNullOrEmpty(host) && !string.IsNullOrWhiteSpace(host))
			{
				var confiruation = new ConfigurationOptions()
				{
					ServiceName = "DotnetSpider",
					Password = password,
					ConnectTimeout = 65530,
					KeepAlive = 8,
					ConnectRetry = 20,
					SyncTimeout = 65530,
					ResponseTimeout = 65530
				};
#if NET_CORE
				if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					// Lewis: This is a Workaround for .NET CORE can't use EndPoint to create Socket.
					var address = Dns.GetHostAddressesAsync(host).Result.FirstOrDefault();
					if (address == null)
					{
						throw new SpiderException("Can't resovle your host: " + host);
					}
					confiruation.EndPoints.Add(new IPEndPoint(address, port));
				}
				else
				{
					confiruation.EndPoints.Add(new DnsEndPoint(host, port));
				}
#else
				confiruation.EndPoints.Add(new DnsEndPoint(host, port));
#endif
				var redis = ConnectionMultiplexer.Connect(confiruation);
				redis.GetSubscriber().Publish($"{identity}", "EXIT");
				return "OK";
			}

			return "REDIS Host is incorrect.";
		}


		private List<TaskStatus> GetItems(IDbConnection conn, int page, int pageSize)
		{
			return conn.Query<TaskStatus>($"SELECT * FROM nlog.status ORDER BY id DESC LIMIT {(page - 1) * pageSize},{pageSize}").ToList();
		}
	}
}
