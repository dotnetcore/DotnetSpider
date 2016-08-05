using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Java2Dotnet.Spider.Extension;
using Java2Dotnet.Spider.Test.Example;
using Java2Dotnet.Spider.Test.Pipeline;
using Java2Dotnet.Spider.Log;
using Newtonsoft.Json;
using Java2Dotnet.Spider.Core.Downloader;
using System.Threading.Tasks;
using System.Threading;
using Java2Dotnet.Spider.Core;
using System.Collections.Generic;
using Java2Dotnet.Spider.Common;
using System.Text.RegularExpressions;
using System.Linq;
using Java2Dotnet.Spider.Extension.Configuration;
using Java2Dotnet.Spider.Extension.Configuration.Json;
using Java2Dotnet.Spider.Ioc;
using Java2Dotnet.Spider.Core.Monitor;
using Java2Dotnet.Spider.Extension.Monitor;
using System.Net;
using Java2Dotnet.Spider.Extension.ORM;
using Java2Dotnet.Spider.Extension.Model.Attribute;
using Java2Dotnet.Spider.Extension.Model;
using static Java2Dotnet.Spider.Extension.Configuration.BaseDbPrepareStartUrls;

namespace Java2Dotnet.Spider.Test
{
	public class Program
	{
		public static void Main(string[] args)
		{
			ServiceProvider.Add<ILogService>(new LogService(new ConsoleLog(), new FileLog()));
			ServiceProvider.Add<IMonitorService>(new ConsoleMonitor());
			ServiceProvider.Add<IMonitorService>(new FileMonitor());
			ServiceProvider.Add<IMonitorService>(new HttpMonitor(ConfigurationManager.Get("statusHost")));

			//var start = DateTime.Now;
			JdSkuSampleSpider spiderBuilder = new JdSkuSampleSpider();
			spiderBuilder.Run("rerun");
			//var end = DateTime.Now;
			//Console.WriteLine((end - start).TotalMilliseconds);
			//Console.Read();
			//SpiderExample.Run();
			//JdSkuSampleSpider spiderBuilder = new JdSkuSampleSpider();
			//var context = spiderBuilder.GetBuilder().Context;
			//ContextSpider spider = new ContextSpider(context);
			//spider.Run("rerun");


			//EmailClient client = new EmailClient("smtp.163.com", "modestmt@163.com", "zousong.88");
			//var msg = new EmaillMessage("test", "test", "zlzforever@163.com" );
			//client.SendMail(msg); 
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
				Machine = SystemInfo.HostName,
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
