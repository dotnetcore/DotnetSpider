using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using System;
using System.IO;
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

			Startup.Run("-s:BaiduSearchSpider", "-tid:BaiduSearchSpider", "-i:guid");

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


		private static void MyTest()
		{
			Site site = new Site();
			site.DownloadFiles = true;
			site.Headers = new System.Collections.Generic.Dictionary<string, string>
			{
				{ "Cache-Control","no-cache" },
				{ "Pragma","no-cache" },
				{ "Accept","image/webp,image/apng,image/*,*/*;q=0.8" },
				{ "Accept-Encoding", "gzip, deflate, br" },
				{ "Accept-Language", "zh-CN,zh;q=0.8" }
			};
			var spider = new DefaultSpider("test", site);
			var request = new Request("https://cdn.dongmanmanhua.cn/15083718553482243295.jpg?x-oss-process=image/quality,q_90");
			request.Referer = "https://www.dongmanmanhua.cn/fantasy/the-god-of-high-school/%E7%AC%AC6%E9%83%A8-re%E4%B8%8E%E7%A5%9E%E7%9A%84%E8%BE%83%E9%87%8F-%E7%AC%AC326%E8%AF%9D/viewer?title_no=224&episode_no=329";
			var downloader = new HttpClientDownloader();

			int downloadCount = 0;
			var filePath = Path.Combine(AppContext.BaseDirectory, @"download\test\15083718553482243295.jpg");
			for (int i = 0; i < 10; ++i)
			{
				downloader.Download(request, spider);
				if (File.Exists(filePath))
				{
					File.Delete(filePath);
					downloadCount++;
				}
			}
			Console.WriteLine($"Download picture success: {downloadCount}");
		}
	}

}
