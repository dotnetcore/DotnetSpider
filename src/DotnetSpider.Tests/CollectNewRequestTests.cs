using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace DotnetSpider.Tests;

public class CollectNewRequestTests
{
    public class TestSpider(
        IOptions<SpiderOptions> options,
        DependenceServices services,
        ILogger<Spider> logger)
        : Spider(options, services, logger)
    {
        public static readonly HashSet<string> CompletedUrls = [];

        public static async Task RunAsync()
        {
            var builder = Builder.CreateDefaultBuilder<TestSpider>(x =>
            {
                x.Speed = 1;
                x.EmptySleepTime = 5;
            });
            await builder.Build().RunAsync();
        }

        class MyDataParser : DataParser
        {
            protected override Task ParseAsync(DataFlowContext context)
            {
                var request = context.Request;

                lock (CompletedUrls)
                {
                    var url = request.RequestUri.ToString();
                    CompletedUrls.Add(url);
                    if (url == "http://baidu.com/")
                    {
                        context.AddFollowRequests(new[] { new Uri("http://cnblogs.com") });
                    }
                }


                return Task.CompletedTask;
            }

            public override Task InitializeAsync()
            {
                return Task.CompletedTask;
            }
        }

        protected override async Task InitializeAsync(CancellationToken stoppingToken = default)
        {
            await AddRequestsAsync(new Request("http://baidu.com"));
            AddDataFlow<MyDataParser>();
        }
    }

    [Fact]
    public async Task CollectNewRequest()
    {
        await TestSpider.RunAsync();

        Assert.Equal(2, TestSpider.CompletedUrls.Count);
    }
}
