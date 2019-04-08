using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetSpider.Data.Parser;
using DotnetSpider.Data.Storage;
using DotnetSpider.Downloader;
using DotnetSpider.Kafka;

namespace DotnetSpider.Sample.samples
{
	public class DistributedSpider
	{
		public static Task Run()
		{
			var builder = new SpiderBuilder();
			builder.AddSerilog();
			builder.ConfigureAppConfiguration();
			builder.UserKafka();
			var provider = builder.Build();

			var spider = provider.Create<Spider>();

			spider.Id = Guid.NewGuid().ToString("N"); // 设置任务标识
			spider.Name = "博客园全站采集"; // 设置任务名称
			spider.Speed = 2; // 设置采集速度, 表示每秒下载多少个请求, 大于 1 时越大速度越快, 小于 1 时越小越慢, 不能为0.
			spider.Depth = 3; // 设置采集深度
			spider.DownloaderSettings.Type = DownloaderType.HttpClient; // 使用普通下载器, 无关 Cookie, 干净的 HttpClient
			spider.AddDataFlow(new DataParser<EntitySpider.CnblogsEntry>())
				.AddDataFlow(new SqlServerEntityStorage(StorageType.InsertIgnoreDuplicate, "Data Source=.;Initial Catalog=master;User Id=sa;Password='1qazZAQ!'"));
			spider.AddRequests(
				new Request("https://news.cnblogs.com/n/page/1/", new Dictionary<string, string> {{"网站", "博客园"}}),
				new Request("https://news.cnblogs.com/n/page/2/", new Dictionary<string, string> {{"网站", "博客园"}}));
			return spider.RunAsync(); // 启动
		}
	}
}