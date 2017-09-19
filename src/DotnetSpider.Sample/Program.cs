using DotnetSpider.Core;
using System;
using System.Net;
#if !NETCOREAPP2_0
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
#if NETCOREAPP2_0
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#else
			ThreadPool.SetMinThreads(200, 200);
			OcrDemo.Process();
#endif

			MyTest();

			Startup.Run("-s:DotnetSpider.Sample.BaiduSearchSpider", "-tid:BaiduSearch", "-i:guid", "-a:");

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
			//http://www.zzpzh.com/dealer/dealer.asp?country=%D6%D0%B9%FA&pro=%C9%CF%BA%A3&city=&page=2
			var str = utf8_gb2312("中国");

		}

		/// <summary>
		/// UTF8转换成GB2312
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string utf8_gb2312(string text)
		{
			//声明字符集   
			System.Text.Encoding utf8, gb2312;
			//utf8   
			utf8 = System.Text.Encoding.GetEncoding("utf-8");
			//gb2312   
			gb2312 = System.Text.Encoding.GetEncoding("gb2312");
			byte[] utf;
			utf = utf8.GetBytes(text);
			utf = System.Text.Encoding.Convert(utf8, gb2312, utf);
			//返回转换后的字符   
			return gb2312.GetString(utf);
		}
	}

}
