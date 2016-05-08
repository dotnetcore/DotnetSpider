using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Java2Dotnet.Spider.Extension;
using Java2Dotnet.Spider.Test.Example;
using Java2Dotnet.Spider.Test.Pipeline;
using Java2Dotnet.Spider.JLog;
using Newtonsoft.Json;
using Java2Dotnet.Spider.Core.Downloader;
using System.Threading.Tasks;
using System.Threading;
using Java2Dotnet.Spider.Core;
using System.Collections.Generic;
using Java2Dotnet.Spider.Common;
using System.Text.RegularExpressions;
using Dapper;
using System.Linq;

namespace Java2Dotnet.Spider.Test
{
	public class Program
	{
		public static void Main(string[] args)
		{
			//JdSkuSampleSpider spiderBuilder = new JdSkuSampleSpider();
			//var context = spiderBuilder.GetBuilder().Context;
			//ContextSpider spider = new ContextSpider(context);
			//spider.Run("rerun");


			//EmailClient client = new EmailClient("smtp.163.com", "modestmt@163.com", "zousong.88");
			//var msg = new EmaillMessage("test", "test", "zlzforever@163.com" );
			//client.SendMail(msg);
			using (var conn = new MySql.Data.MySqlClient.MySqlConnection("Database='test';Data Source= 86research.imwork.net;User ID=root;Password=1qazZAQ!;Port=4306"))
			{
				List<TmallShop> shops = conn.Query<TmallShop>("SELECT `name`,shopid,uid,url,is_tmall,run_id,brand,category,cdate from taobao.tmall_shop where is_tmall=1 or is_tmall='True'", null).ToList();
				conn.Execute("insert ignore into taobao.tmall_shop_weekly_v2 (`name`,shopid,uid,url,is_tmall,run_id,brand,category,cdate) values(@Name,@ShopId,@UserId,@Url,@Is_Tmall,@Run_Id,@Brand,@Category,@CDate)", shops);
			}
		}

		public class TmallShop
		{
			public string Name { get; set; }

			public string ShopId { get; set; }

			public string UserId { get; set; }

			public string Url { get; set; }

			public string Is_Tmall { get; set; }

			public DateTime Run_Id { get; set; }

			public string Brand { get; set; }

			public string Category { get; set; }

			public DateTime CDate { get; set; }
		}

		private static void TestStatusServer()
		{
			var ErrorPageCount = 10;
			var LeftPageCount = 10;
			var PagePerSecond = 10;
			var StartTime = new DateTime(2016, 4, 19);
			var EndTime = DateTime.Now;
			var Status = "running";
			var SuccessPageCount = 100;
			var ThreadCount = 5;
			var TotalPageCount = 1000;
			var AliveThreadCount = 5;
			var Name = "Tmall Gmv Monthly " + DateTime.Now.ToString("yyyy-MM-dd");

			var status = new
			{
				Message = new
				{
					ErrorPageCount,
					LeftPageCount,
					PagePerSecond,
					StartTime,
					EndTime,
					Status,
					SuccessPageCount,
					ThreadCount,
					TotalPageCount,
					AliveThreadCount
				},
				Name,
				Machine = Log.Machine,
				UserId = "DotnetSpider",
				TaskGroup = "Tmall Gmv Monthly"
			};

			HttpClient client = new HttpClient();

			var task = client.PostAsync("http://localhost:62823/api/status/uploadstatus", new StringContent(JsonConvert.SerializeObject(status), Encoding.UTF8, "application/json"));
			var r = task.Result;
		}

		static void Run(Action a, string type, string name)
		{
			try
			{
				a();
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write($"[{type}] [{name}] ");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("PASS.");
				Console.WriteLine();
			}
			catch (Exception e)
			{
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write($"[{type}] [{name}] ");
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("FAILD.");
				Console.WriteLine();
			}
		}
	}
}
