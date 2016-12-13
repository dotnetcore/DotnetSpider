using DotnetSpider.Core;
using DotnetSpider.Core.Monitor;
using DotnetSpider.Redial;
using DotnetSpider.Redial.InternetDetector;
using DotnetSpider.Redial.Redialer;

namespace DotnetSpider.Sample
{
	public class Program
	{
		public static void Main(string[] args)
		{
			IocManager.AddSingleton<IMonitor, NLogMonitor>();

			//BaseUsage.Run();



			BaiduSearchSpider casSper = new BaiduSearchSpider();
			casSper.Run();
		}
	}
}
