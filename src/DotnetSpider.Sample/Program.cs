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
			IocContainer.Default.AddSingleton<IMonitor, NLogMonitor>();

			JdSkuSampleSpider spider = new JdSkuSampleSpider();
			spider.Run();
		}
	}
}
