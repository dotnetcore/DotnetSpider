using DotnetSpider.Core;
using DotnetSpider.Core.Monitor;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.Sample
{
	public class Program
	{
		public static void Main(string[] args)
		{
			IocExtension.ServiceCollection.AddSingleton<IMonitorService, NLogMonitor>();
#if NET_CORE
            JdSkuSampleSpider spiderBuilder = new JdSkuSampleSpider();
#else
            GSXSpider spiderBuilder = new GSXSpider();
#endif
            spiderBuilder.Run("rerun");
		}
	}
}
