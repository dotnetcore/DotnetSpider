//using System.Text;
//using System.Threading;
//using DotnetSpider.Core.Monitor;

//namespace DotnetSpider.Core.Test
//{
//	public class Program
//	{
//		public static void Main(string[] args)
//		{
//#if NET_CORE
//			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
//#endif

//			IocManager.Register<IMonitor, NLogMonitor>();

//			Spider spider = Spider.Create(new Site { EncodingName = "UTF-8", MinSleepTime = 1000 }, new SpiderTest.TestPageProcessor()).AddPipeline(new SpiderTest.TestPipeline()).SetThreadNum(1);
//			spider.SetDownloader(new TestDownloader());
//			for (int i = 0; i < 10; i++)
//			{
//				spider.AddStartUrl("http://www.baidu.com/" + i);
//			}
//			spider.Run();
//			Thread.Sleep(5000);
//			spider.Stop();
//			Thread.Sleep(5000);
//			spider.RunAsync();
//			Thread.Sleep(5000);
//		}
//	}
//}
