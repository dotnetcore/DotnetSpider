using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using DotnetSpider.Core;
using DotnetSpider.Data.Parser;
using DotnetSpider.Data.Storage;
using DotnetSpider.Downloader;
using DotnetSpider.Kafka;
using DotnetSpider.Sample.samples;


namespace DotnetSpider.Sample
{
	class Program
	{
		static async Task Main(string[] args)
		{
			await BaseUsage.Run();

//			var builder = new SpiderBuilder();
//			builder.AddSerilog();
//			builder.ConfigureAppConfiguration();
//			builder.UserKafka();
//			var provider = builder.Build();
//			var spider = provider.Create<Spider>();
//
//			spider.Id = Guid.NewGuid().ToString("N"); // 设置任务标识
//			spider.Name = "博客园全站采集"; // 设置任务名称
//			spider.Speed = 1; // 设置采集速度, 表示每秒下载多少个请求, 大于 1 时越大速度越快, 小于 1 时越小越慢, 不能为0.
//			spider.Depth = 3; // 设置采集深度
//			spider.DownloaderSettings.Type = DownloaderType.HttpClient; // 使用普通下载器, 无关 Cookie, 干净的 HttpClient
//			spider.AddDataFlow(new DataParser
//			{
//				SelectableFactory = context => context.GetSelectable(ContentType.Html),
//				CanParse = DataParserHelper.CanParseByRegex("cnblogs\\.com"),
//				QueryFollowRequests = DataParserHelper.QueryFollowRequestsByXPath(".")
//			}).AddDataFlow(new ConsoleStorage()); // 控制台打印采集结果
//			spider.AddRequests("http://www.cnblogs.com/"); // 设置起始链接
//			await spider.RunAsync(); // 启动
			Console.Read();
		}
	}
}