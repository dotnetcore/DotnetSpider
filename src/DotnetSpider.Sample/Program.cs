using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

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
            //  注册了CodePages 编码则为UTF-8
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#else
			ThreadPool.SetMinThreads(200, 200);
			OcrDemo.Process();
#endif
            //MyTest();

            //Startup.Run("-s:BaiduSearchSpider", "-tid:1", "-i:guid");

            //Startup.Run("-s:DotnetSpider.Sample.CustomSpider1", "-tid:CustomSpider1", "-i:CustomSpider1");

            //Startup.Run("-s:DotnetSpider.Sample.DefaultMySqlPipelineSpider", "-tid:DefaultMySqlPipeline", "-i:guid", "-a:");

            ////ConfigurableSpider.Run();

            //// Custmize processor and pipeline 完全自定义页面解析和数据管道
            //BaseUsage.CustmizeProcessorAndPipeline();
            //Console.WriteLine("Press any key to continue...");
            //Console.Read();

            //// Crawler pages without traverse 采集指定页面不做遍历
            //BaseUsage.CrawlerPagesWithoutTraverse();
            //Console.WriteLine("Press any key to continue...");
            //Console.Read();

            //// Crawler pages traversal 遍历整站
            //BaseUsage.CrawlerPagesTraversal();
            //Console.WriteLine("Press any key to continue...");
            //Console.Read();

            //DDengEntitySpider dDengEntitySpider = new DDengEntitySpider();
            //dDengEntitySpider.Run();
            //Console.WriteLine("Press any key to continue...");
            //Console.Read();

            //Cnblogs.Run();
            //Console.WriteLine("Press any key to continue...");
            //Console.Read();

            ////CasSpider casSpider = new CasSpider();
            ////casSpider.Run();
            ////Console.WriteLine("Press any key to continue...");
            ////Console.Read();

            //BaiduSearchSpider baiduSearchSpider = new BaiduSearchSpider();
            //baiduSearchSpider.Run();
            //Console.WriteLine("Press any key to continue...");
            //Console.Read();

            //JdSkuSampleSpider jdSkuSampleSpider = new JdSkuSampleSpider();
            //jdSkuSampleSpider.Run();
            //Console.WriteLine("Press any key to continue...");
            //Console.Read();

            //LyProductSpider lyProductSpider = new LyProductSpider();
            //lyProductSpider.Run();
            //Console.WriteLine("Press any key to continue...");
            //Console.Read();



            ZhiPinSpider spider = new ZhiPinSpider();
            spider.Run();
            Console.WriteLine("Press any key to continue...");
            Console.Read();



            //Situoli.Run();



        }


        private static void MyTest()
		{
			Dictionary<string, string> tmp = new Dictionary<string, string>();
			tmp.Add("a", "b");
			tmp.Add("b", "c");

			var o = JsonConvert.SerializeObject(tmp);
		}
	}

}
