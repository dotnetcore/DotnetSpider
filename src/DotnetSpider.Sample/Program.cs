using System;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Sample.samples;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.Sample;

class Program
{
    static async Task Main(string[] args)
    {
        ThreadPool.SetMaxThreads(255, 255);
        ThreadPool.SetMinThreads(255, 255);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console().WriteTo.RollingFile("logs/spider.log")
            .CreateLogger();

        // var builder = new HostBuilder();
        // builder.ConfigureServices(collection =>
        // {
        //     // 添加统计后台服务
        //     collection.AddStatisticHostService<InMemoryStatisticStore>();
        //     // 添加本地消息队列
        //     collection.AddLocalMQ();
        //     // 添加本地下载节点
        //     collection.AddAgentHostService();
        //     // 添加本地下载中心
        //     collection.AddAgentCenterHostService<InMemoryAgentStore>();
        //     collection.AddCoreServices();
        // }).UseSerilog();
        // await builder.Build().RunAsync();

        // // await DistributedSpider.RunAsync();
        // await ProxySpider.RunAsync();
        // await EntitySpider.RunMySqlQueueAsync();

        await CnBlogsSpider.RunAsync();

        Console.WriteLine("Bye!");
    }
}
