using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using System;
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
			site.CookiesStringPart = "RK=EctiCBw6Qu; tvfe_boss_uuid=679b4f85937a1ceb; pgv_pvi=7677072384; pac_uid=1_136831898; hjstat_uv=5942539673624870773|679544; gaduid=59dc77703003d; o_cookie=136831898; theme=dark; roastState=1; pgv_si=s386453504; _qpsvr_localtk=0.3831488783510728; ptui_loginuin=3283360259; ptisp=ctc; ptcz=99b83efa1172fbab4ef8f554b0cba6deca39a76c635fbfec20605afc77cccd8e; uin=o3283360259; skey=@N2kn0txfX; pt2gguin=o3283360259; p_uin=o3283360259; pt4_token=K4InOhYERnMakvbcOR7MB15xpde5Mg3JYz8*6rhWit4_; p_skey=ns3wPjqy66svoaZUaQUw3gi1x5-VWMaZ3aEDqHDiYCc_; lw_user_info=%7B%22uin%22%3A%223283360259%22%2C%22nick%22%3A%22%5Cu9ec4%5Cu5c71%5Cu6bdb%5Cu5c16%22%2C%22head%22%3A%22http%3A%5C%2F%5C%2Fthirdqq.qlogo.cn%5C%2Fg%3Fb%3Dsdk%26k%3DcqM5xBTMeVV8PFogl2PnXw%26s%3D640%26t%3D1483436537%22%7D; readRecord=%5B%5B621702%2C%22%E6%97%A0%E5%90%8D%E5%95%86%E5%BA%97%22%2C102%2C%22%E7%81%AF%EF%BC%8C%E7%BB%9D%E5%AF%B9%E4%B8%8D%E5%8F%AF%E4%BB%A5%E5%85%B3%22%2C97%5D%5D; readLastRecord=%5B%5D; pgv_info=ssid=s3668706308; ts_last=ac.qq.com/ComicView/index/id/621702/cid/102/auth/1; pgv_pvid=242838150; ts_uid=9047984164";
			site.Headers = new System.Collections.Generic.Dictionary<string, string>
			{
				{ "Accept","application/json, text/javascript, */*; q=0.01" },
				{ "Accept-Encoding", "gzip, deflate" },
				{ "Accept-Language", "zh-CN,zh;q=0.8" },
				{ "Content-Type","application/x-www-form-urlencoded; charset=UTF-8" }
			};
			var spider = new DefaultSpider("test", site);
			var request = new Request("http://ac.qq.com/Buy/buyComic");
			request.Method = HttpMethod.Post;
			request.Referer = "http://ac.qq.com/Buy/chapterBuyPage/buy_type/1/id/621702/cid/102?id===621702&cid===102&theme===dark&token===6po9oJ4vSkwBlc5QjdZ58aGlhZIgJaW0D6Nf5BCTYHT/at3JbUZSaw9+G5vQ9CTdAqOcK7+VkEOD1frJzqEz7GOebk+n9XRBvX6Dja+MD4g=&pageType===1";
			request.Origin = "http://ac.qq.com";
			var chapter = 102; //已购买
			request.PostBody = $"chapter_id={chapter}&tokenKey=6po9oJ4vSkwBlc5QjdZ58aGlhZIgJaW0D6Nf5BCTYHT%2Fat3JbUZSaw9%2BG5vQ9CTdAqOcK7%2BVkEOD1frJzqEz7GOebk%2Bn9XRBvX6Dja%2BMD4g%3D&comic_id=621702&buy_type=1&pay_type=1&auto_buy_next=1&channel=&app_id=1450011607&all_chapter=1&uin=o3283360259&skey=%40N2kn0txfX";
			var downloader = new HttpClientDownloader();
			var content = downloader.Download(request, spider);


			Console.WriteLine($"Result: {content.Content}");
		}
	}

}
