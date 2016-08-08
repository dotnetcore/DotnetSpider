using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using DotnetSpider.Extension;
using DotnetSpider.Test.Example;
using DotnetSpider.Test.Pipeline;
using Newtonsoft.Json;
using DotnetSpider.Core.Downloader;
using System.Threading.Tasks;
using System.Threading;
using DotnetSpider.Core;
using System.Collections.Generic;
using DotnetSpider.Core.Common;
using System.Text.RegularExpressions;
using System.Linq;
using DotnetSpider.Extension.Configuration;
using DotnetSpider.Extension.Configuration.Json;

using DotnetSpider.Core.Monitor;
using DotnetSpider.Extension.Monitor;
using System.Net;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.Test
{
	public class Program
	{
		public static void Main(string[] args)
		{
			IocExtension.ServiceCollection.AddSingleton<IMonitorService, NLogMonitor>();
			IocExtension.ServiceCollection.AddSingleton<IMonitorService, HttpMonitor>();
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
