using DotnetSpider.Test.Example;
using DotnetSpider.Core;
using DotnetSpider.Core.Monitor;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.Test
{
	public class Program
	{
		public static void Main(string[] args)
		{
			IocExtension.ServiceCollection.AddSingleton<IMonitorService, NLogMonitor>();
			//IocExtension.ServiceCollection.AddSingleton<IMonitorService, HttpMonitor>();

			JdSkuSampleSpider spiderBuilder = new JdSkuSampleSpider();
			spiderBuilder.Run("rerun");
			//var end = DateTime.Now;
			//Console.WriteLine((end - start).TotalMilliseconds);
			//Console.Read();
			//SpiderExample.Run();
			//JdSkuSampleSpider spiderBuilder = new JdSkuSampleSpider();
			//var context = spiderBuilder.GetBuilder().Context;
			//ContextSpider spider = new ContextSpider(context);
			//spider.Run("rerun");
		}
	}
}
