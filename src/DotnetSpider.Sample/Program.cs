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

			JdSkuSampleSpider2 spiderBuilder = new JdSkuSampleSpider2();
			spiderBuilder.Run("rerun");
		}
	}
}
