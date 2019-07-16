using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.Downloader;
using DotnetSpider.Kafka;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DotnetSpider.Sample.samples
{
	public class DistributedSpider
	{
		public static Task Run()
		{
			var builder = new SpiderHostBuilder()
				.ConfigureLogging(x => x.AddSerilog())
				.ConfigureAppConfiguration(x => x.AddJsonFile("appsettings.json"))
				.ConfigureServices(services =>
				{
					services.AddKafkaEventBus();
				});
			var provider = builder.Build();

			var spider = provider.Create<Spider>();

			spider.Id = Guid.NewGuid().ToString("N"); // 设置任务标识
			spider.Name = "博客园全站采集"; // 设置任务名称
			spider.Speed = 1; // 设置采集速度, 表示每秒下载多少个请求, 大于 1 时越大速度越快, 小于 1 时越小越慢, 不能为0.
			spider.Depth = 3; // 设置采集深度
			spider.AddDataFlow(new DataParser<EntitySpider.CnblogsEntry>())
				.AddDataFlow(spider.GetDefaultStorage());
			spider.AddRequests(
				new Request("https://news.cnblogs.com/n/page/1/", new Dictionary<string, string> {{"网站", "博客园"}}),
				new Request("https://news.cnblogs.com/n/page/2/", new Dictionary<string, string> {{"网站", "博客园"}}));
			return spider.RunAsync(); // 启动
		}
	}
}