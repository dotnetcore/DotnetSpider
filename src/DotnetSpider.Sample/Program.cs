using DotnetSpider.Core;
using DotnetSpider.Extension;
using MessagePack;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
#if !NETCOREAPP
using System.Threading;
#else
using System.Text;
#endif

namespace DotnetSpider.Sample
{
	public class Program
	{
		public static void Main(string[] args)
		{
#if NETCOREAPP
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#else
			ThreadPool.SetMinThreads(200, 200);
			OcrDemo.Process();
#endif
			SampleSpider.Run();
			MyTest();

			Startup.Run("-s:BaiduSearchSpider", "--tid:1", "-i:guid");

			Startup.Run("-s:DotnetSpider.Sample.CustomSpider1", "-tid:CustomSpider1", "-i:CustomSpider1");

			Startup.Run("-s:DotnetSpider.Sample.DefaultMySqlPipelineSpider", "-tid:DefaultMySqlPipeline", "-i:guid", "-a:");

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

			//CasSpider casSpider = new CasSpider();
			//casSpider.Run();
			//Console.WriteLine("Press any key to continue...");
			//Console.Read();

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


		/// <summary>
		/// <c>MyTest</c> is a method in the <c>Program</c>
		/// </summary>
		private static void MyTest()
		{
			var connection = ConnectionMultiplexer.Connect("127.0.0.1:6379,serviceName=Scheduler.NET,keepAlive=8,allowAdmin=True,connectTimeout=10000,password=,abortConnect=True,connectRetry=20");
			var db = connection.GetDatabase();


			var key = DateTime.Now.ToString("yyyyMMdd");

			var obj = new[] { "30001646", "1217572261", "1217572261", "24", "宏祥家纺专营店", "128", "210", "5769", "1", "2990", "15285", "15250", "15248", "0", "沙发垫套装夏季真皮实木红木欧式麻将凉席沙发垫子坐垫飘窗垫窗台垫子沙发套罩全包巾竹凉垫椅子夏天防滑四季 （支持定做）碳化小提花 / 单牛筋 45 * 45cm", "1114622443", "0", "1", "1", "20971656", "67", "29.9", "0", "268460160", "5", "0", "9699", "0", "0", "98", "0", "0", "0", "75094", "1405324110", "71564", "0", "0", "0", "20180523000001", "0", "24687", "0", "0", "4000", "0", "6", "0", "0", "0", "0", "0", "2018-05-23 00:00:00", "1", "2018-05-23 20:00:15" };

			var str = JsonConvert.SerializeObject(obj);
			var ttt = Encoding.UTF8.GetBytes(str);
			var bytes = LZ4MessagePackSerializer.Typeless.Serialize(obj);
			db.ListRightPush(key, bytes);
			var a = (byte[])db.ListLeftPop(key);
			var d = LZ4MessagePackSerializer.Typeless.Deserialize(a);
		}
	}
}
