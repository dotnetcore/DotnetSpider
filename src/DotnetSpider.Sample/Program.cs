using DotnetSpider.Core;
using DotnetSpider.Core.Redial;
using DotnetSpider.Core.Redial.InternetDetector;
using DotnetSpider.Core.Redial.Redialer;
using DotnetSpider.Extension.Pipeline;
using System;
using System.IO;
using System.Net;
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

			Startup.Run("-s:BaiduSearch", "-tid:BaiduSearch", "-i:guid", "-a:");

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
	 
		}
	}

}
