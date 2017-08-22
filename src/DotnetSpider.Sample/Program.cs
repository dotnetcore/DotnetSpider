using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Extension.Scheduler;
using DotnetSpider.Runner;
using System;
using System.Collections.Concurrent;
using System.Linq;
#if !NETCOREAPP2_0
using System.Threading;
#else
using System.Text;
using System.Text.RegularExpressions;
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
			Regex _tokenRegex = new Regex("token : '[\\w\\d-]+'");
			var website = "9158.com";
			var httpRequest = new HttpRequest
			{
				Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
				Url = $"http://www.alexa.cn/traffic/{website}",
				UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.101 Safari/537.36",
				Cookie = "PHPSESSID=tedb9eqf2bk8r74qevnh2jr0i3; exi_users=ThirxzQHkQzzvNoXvi8MV8JV-D3xcy7y80ZBn39PPi4r6qCq14BTR0nx3lCg6pjaVcAsaaxY-DaxnCNQeuzqo-Dcaqps2W3Mwf-JxEKGW0XlF-DvIPG-DSQUrrCXWS-DnftG1pcZnbuetDsipGs-JW4GIoZNg8o-DDrk-Dl4Q7iMwc9aFXOl00y6IRL4SBAjiAaWPfwHfOdxNE0RuRCt9AtGLwc3ih-DrvgCOyeMG1R5-DChWXxnfwHs6m-DX-J7ywmI1n2Gaw44Y8CFfUAOW2fYX3bqeII5nMiS7ew3GYQnmlFkj8LDuAJgLn0jS4ltvwJtP-Jn2tq234FZ1bpIRuqNIoc2IbPujv8-J9K8oGBHUEq9IJRsgQRoeMaK4klF-JFFKCYDYywOJY0-JnNatWE7-DeFqpLAPmdsf2uME39Kl9XM-JaetwRboaGxWEI-L; exi_query_history=bmCBmubXy40rvL3en9k2IwL6gvOriT4gAvq1mrUZeNYmP69-HN5FowUx7rXFBnRutyA4ZGs3BIEAimryOuVgCQ9CkSQikJelkKLnu86EkrzrpEoGrmYwFjJCNziVyhQU5OPNp16J6Nt4Vn6TPZ3hYHBfPgK0WeTc5eIUipPd-HaCY-K"
			};
			httpRequest.Header.Add("Accept-Encoding", "gzip, deflate");
			httpRequest.Header.Add("Accept-Language", "zh-CN,zh;q=0.8");
			httpRequest.Header.Add("Cache-Control", "max-age=0");
			httpRequest.Encoding = Encoding.UTF8;

			var result = HttpSender.GetHtml(httpRequest);

			var match = _tokenRegex.Match(result.Html);
			var token = match.Value.Replace("token : '", "").Replace("'", "");
		}

	}
}
