using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Runner;
using Newtonsoft.Json.Linq;
using System;
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

			Startup.Run(new string[] { "-s:BaiduSearch", "-tid:BaiduSearch", "-i:guid" });
			return;

			Startup.Run(new string[] { "-s:JdSkuSample", "-tid:JdSkuSample", "-i:guid" });

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
			//HttpHelper helper = new HttpHelper();
			//var result = helper.GetHtml(new HttpItem
			//{
			//	Url = "https://amos.alicdn.com/muliuserstatus.aw?_ksTS=1502446000449_784&callback=jsonp785&beginnum=0&charset=utf-8&uids=%E4%B8%83%E5%8C%B9%E7%8B%BC%E5%B0%8A%E6%82%A6%E4%B8%93%E5%8D%96%E5%BA%97;%E6%A3%89%E5%93%81%E4%B8%96%E5%AE%B6%E5%86%85%E8%A1%A3%E4%B8%93%E8%90%A5%E5%BA%97;%E5%98%89%E5%A3%AB%E5%86%85%E8%A1%A3%E4%B8%93%E8%90%A5%E5%BA%97;%E8%8C%B5%E4%B9%8B%E8%8B%A5%E6%9C%8D%E9%A5%B0%E4%B8%93%E8%90%A5%E5%BA%97;%E8%B6%85%E4%B9%8B%E7%BE%A4%E6%9C%8D%E9%A5%B0%E4%B8%93%E8%90%A5%E5%BA%97;%E5%BD%BC%E5%B0%94%E4%B8%B9%E6%97%97%E8%88%B0%E5%BA%97;%E5%AF%B9%E5%AF%B9%E7%86%8A%E6%9C%8D%E9%A5%B0%E4%B8%93%E8%90%A5%E5%BA%97;%E8%BF%AA%E9%82%A6%E4%BB%95%E6%9C%8D%E9%A5%B0%E5%88%B6%E8%A1%A3%E5%8E%82;%E7%A7%91%E5%B0%86%E6%9C%8D%E9%A5%B0%E6%97%97%E8%88%B0%E5%BA%97;%E6%AC%A7%E9%98%B3%E7%BE%8E%E5%86%85%E8%A3%A4%E5%88%B6%E8%A1%A3%E5%8E%82&site=cntaobao"
			//});
		}
	}
}
