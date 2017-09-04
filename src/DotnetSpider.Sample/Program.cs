using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Extension.Infrastructure;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
#if !NETCOREAPP2_0
using System.Threading;
#else
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endif

namespace DotnetSpider.Sample
{
	public class Program
	{
		public static void Main(string[] args)
		{
#if NETCOREAPP2_0
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#else
			ThreadPool.SetMinThreads(200, 200);
			OcrDemo.Process();
#endif

			MyTest();

			Startup.Run(new string[] { "-s:BaiduSearch", "-tid:BaiduSearch", "-i:guid", "-a:" });

			Startup.Run(new string[] { "-s:CustomSpider1", "-tid:CustomSpider1", "-i:CustomSpider1" });

			//ConfigurableSpider.Run();

			// Custmize processor and pipeline 完全自定义页面解析和数据管道
			BaseUsage.CustmizeProcessorAndPipeline();
			Console.WriteLine("Press any key to continue...");
			Console.Read();

			// Crawler pages without traverse 采集指定页面不做遍历
			BaseUsage.CrawlerPagesWithoutTraverse();
			Console.WriteLine("Press any key to continue...");
			Console.Read();

			// Crawler pages traversal 遍历整站
			BaseUsage.CrawlerPagesTraversal();
			Console.WriteLine("Press any key to continue...");
			Console.Read();

			DDengEntitySpider dDengEntitySpider = new DDengEntitySpider();
			dDengEntitySpider.Run();
			Console.WriteLine("Press any key to continue...");
			Console.Read();

			Cnblogs.Run();
			Console.WriteLine("Press any key to continue...");
			Console.Read();

			CasSpider casSpider = new CasSpider();
			casSpider.Run();
			Console.WriteLine("Press any key to continue...");
			Console.Read();

			BaiduSearchSpider baiduSearchSpider = new BaiduSearchSpider();
			baiduSearchSpider.Run();
			Console.WriteLine("Press any key to continue...");
			Console.Read();

			JdSkuSampleSpider jdSkuSampleSpider = new JdSkuSampleSpider();
			jdSkuSampleSpider.Run();
			Console.WriteLine("Press any key to continue...");
			Console.Read();

			Situoli.Run();
		}


		private static void MyTest()
		{
			using (var conn = new MySqlConnection("Database='mysql';Data Source=192.168.90.100;User ID=admin;Password=YO3brdgpXvjzEl*b*qSZkFTyN*XF$P65;Port=53306;SslMode=None"))
			{
				var results = conn.Query<A>("SELECT * FROM taobao.brand_tracking_items_sold WHERE price is null").ToList();

				foreach (var item in results)
				{
					item.price = float.Parse(item.sku_price.Split(' ').First());
				}

				conn.Execute("UPDATE taobao.brand_tracking_items_sold SET price=@price WHERE __id=@__id;", results);
			}
		}

		class A
		{
			public string sku_price { get; set; }
			public float price { get; set; }
			public int __id { get; set; }
		}

		private static void Spider_OnClosed(Spider spider)
		{
			Console.WriteLine($"Spider: {spider.Identity} closed, Status: {spider.Stat}");
		}
	}

}
