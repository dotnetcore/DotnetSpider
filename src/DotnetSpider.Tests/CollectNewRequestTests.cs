using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.Downloader;
using DotnetSpider.Http;
using DotnetSpider.Scheduler;
using DotnetSpider.Scheduler.Component;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace DotnetSpider.Tests;

public class CollectNewRequestTests
{
    public class TestSpider : Spider
    {
        public static readonly HashSet<string> CompletedUrls = new();

        public static async Task RunAsync()
        {
            var builder = Builder.CreateDefaultBuilder<TestSpider>(x =>
            {
                x.Speed = 1;
                x.EmptySleepTime = 5;
            });
            // builder.UseDownloader<HttpClientDownloader>();
            builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
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

        public TestSpider(IOptions<SpiderOptions> options, DependenceServices services,
            ILogger<Spider> logger) : base(
            options, services, logger)
        {
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
