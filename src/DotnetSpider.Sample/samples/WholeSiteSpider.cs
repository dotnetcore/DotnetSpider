using System;
using System.Threading.Tasks;
using DotnetSpider.Common;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.DataFlow.Storage.Mongo;
using DotnetSpider.Downloader;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DotnetSpider.Sample.samples
{
	public class WholeSiteSpider
	{
		public static void Run1()
		{
			var builder = new SpiderHostBuilder()
				.ConfigureLogging(x => x.AddSerilog())
				.ConfigureAppConfiguration(x => x.AddJsonFile("appsettings.json"))
				.ConfigureServices(services =>
				{
					services.AddLocalEventBus();
					services.AddLocalDownloadCenter();
					services.AddDownloaderAgent(x =>
					{
						x.UseFileLocker();
						x.UseDefaultAdslRedialer();
						x.UseDefaultInternetDetector();
					});
					services.AddStatisticsCenter(x => x.UseMemory());
				});

			var provider = builder.Build();
			var spider = provider.Create<Spider>();

			spider.Id = Guid.NewGuid().ToString("N"); // 设置任务标识
			spider.Name = "博客园全站采集"; // 设置任务名称
			spider.Speed = 1; // 设置采集速度, 表示每秒下载多少个请求, 大于 1 时越大速度越快, 小于 1 时越小越慢, 不能为0.
			spider.Depth = 3; // 设置采集深度
			var parser = new DataParser
			{
				SelectableBuilder = context => context.Response.ToSelectable(ContentType.Html),
				Required = DataParserHelper.CheckIfRequiredByRegex("cnblogs\\.com")
			};
			parser.SetFollowRequestQuerier(DataParserHelper.QueryFollowRequestsByXPath("."));
			spider.AddDataFlow(parser).AddDataFlow(new ConsoleStorage()); // 控制台打印采集结果
			spider.AddRequests("http://www.cnblogs.com/"); // 设置起始链接
			spider.RunAsync().GetAwaiter().GetResult(); // 启动
		}

		public static Task Run2()
		{
			var builder = new SpiderHostBuilder()
				.ConfigureLogging(x => x.AddSerilog())
				.ConfigureAppConfiguration(x => x.AddJsonFile("appsettings.json"))
				.ConfigureServices(services =>
				{
					services.AddLocalEventBus();
					services.AddLocalDownloadCenter();
					services.AddDownloaderAgent(x =>
					{
						x.UseFileLocker();
						x.UseDefaultAdslRedialer();
						x.UseDefaultInternetDetector();
					});
					services.AddStatisticsCenter(x => x.UseMemory());
				}).Register<EntitySpider>();
			var provider = builder.Build();
			var spider = provider.Create<Spider>();
			spider.Id = Guid.NewGuid().ToString("N"); // 设置任务标识
			spider.Name = "博客园全站采集"; // 设置任务名称
			spider.Speed = 1; // 设置采集速度, 表示每秒下载多少个请求, 大于 1 时越大速度越快, 小于 1 时越小越慢, 不能为0.
			spider.Depth = 3; // 设置采集深度
			var options = provider.GetRequiredService<SpiderOptions>();
			spider.AddDataFlow(new CnblogsDataParser())
				.AddDataFlow(new MongoEntityStorage(options.StorageConnectionString));
			spider.AddRequests("http://www.cnblogs.com/"); // 设置起始链接
			return spider.RunAsync(); // 启动
		}

		class CnblogsDataParser : DataParser
		{
			public CnblogsDataParser()
			{
				Required = DataParserHelper.CheckIfRequiredByRegex("cnblogs\\.com");
				FollowRequestQuerier = BuildFollowRequestQuerier(DataParserHelper.QueryFollowRequestsByXPath("."));
			}

			protected override Task<DataFlowResult> Parse(DataFlowContext context)
			{
				context.AddData("URL", context.Response.Request.Url);
				context.AddData("Title", context.Selectable.XPath(".//title").GetValue());
				return Task.FromResult(DataFlowResult.Success);
			}
		}
	}
}