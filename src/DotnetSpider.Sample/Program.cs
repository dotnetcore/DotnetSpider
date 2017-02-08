using DotnetSpider.Core;
using DotnetSpider.Core.Monitor;
using DotnetSpider.Redial;
using DotnetSpider.Redial.InternetDetector;
using DotnetSpider.Redial.Redialer;
using System;

namespace DotnetSpider.Sample
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CrawlerHtml.Run();

			Console.WriteLine("Press any key to continue...");
			Console.Read();

			BaseUsage.Run();
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

			JdShopDetailSpider jdShopDetailSpider = new JdShopDetailSpider();
			jdShopDetailSpider.Run();
			Console.WriteLine("Press any key to continue...");
			Console.Read();

			JdSkuSampleSpider jdSkuSampleSpider = new JdSkuSampleSpider();
			jdSkuSampleSpider.Run();
			Console.WriteLine("Press any key to continue...");
			Console.Read();
		}
	}
}
