using System;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Data;
using DotnetSpider.Data.Parser;
using DotnetSpider.Data.Storage;
using DotnetSpider.Downloader;

namespace DotnetSpider.Sample.samples
{
    public class WholeSiteSpider
    {
        public static void Run1()
        {
            var builder = new SpiderBuilder();
            builder.AddSerilog();
            builder.ConfigureAppConfiguration();
            builder.UseStandalone();
            var provider = builder.Build();
            var spider = provider.Create<Spider>();

            spider.Id = Guid.NewGuid().ToString("N"); // 设置任务标识
            spider.Name = "博客园全站采集"; // 设置任务名称
            spider.Speed = 1; // 设置采集速度, 表示每秒下载多少个请求, 大于 1 时越大速度越快, 小于 1 时越小越慢, 不能为0.
            spider.Depth = 3; // 设置采集深度
            spider.DownloaderSettings.Type = DownloaderType.HttpClient; // 使用普通下载器, 无关 Cookie, 干净的 HttpClient
            spider.AddDataFlow(new DataParser
            {
                SelectableFactory = context => context.GetSelectable(ContentType.Html),
                CanParse = DataParserHelper.CanParseByRegex("cnblogs\\.com"),
                QueryFollowRequests =  DataParserHelper.QueryFollowRequestsByXPath(".")
            }).AddDataFlow(new ConsoleStorage()); // 控制台打印采集结果
            spider.AddRequests("http://www.cnblogs.com/"); // 设置起始链接
            spider.RunAsync(); // 启动
        }

        public static Task Run2()
        {
            var builder = new SpiderBuilder();
            builder.AddSerilog();
            builder.ConfigureAppConfiguration();
            builder.UseStandalone();
            builder.AddSpider<EntitySpider>();
            var provider = builder.Build();
            var spider = provider.Create<Spider>();
            spider.Id = Guid.NewGuid().ToString("N"); // 设置任务标识
            spider.Name = "博客园全站采集"; // 设置任务名称
            spider.Speed = 1; // 设置采集速度, 表示每秒下载多少个请求, 大于 1 时越大速度越快, 小于 1 时越小越慢, 不能为0.
            spider.Depth = 3; // 设置采集深度
            spider.DownloaderSettings.Type = DownloaderType.HttpClient; // 使用普通下载器, 无关 Cookie, 干净的 HttpClient
            var options = provider.GetRequiredService<ISpiderOptions>();
            spider.AddDataFlow(new CnblogsDataParser()).AddDataFlow(new MongoEntityStorage(options.StorageConnectionString));
            spider.AddRequests("http://www.cnblogs.com/"); // 设置起始链接
            return spider.RunAsync(); // 启动
        }

        class CnblogsDataParser : DataParser
        {
            public CnblogsDataParser()
            {
                CanParse = DataParserHelper.CanParseByRegex("cnblogs\\.com");
                QueryFollowRequests = DataParserHelper.QueryFollowRequestsByXPath(".");
            }

            protected override Task<DataFlowResult> Parse(DataFlowContext context)
            {
                context.AddItem("URL", context.Response.Request.Url);
                context.AddItem("Title", context.GetSelectable().XPath(".//title").GetValue());
                return Task.FromResult(DataFlowResult.Success);
            }
        }
    }
}