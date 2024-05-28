using System;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.Downloader;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using DotnetSpider.Scheduler;
using DotnetSpider.Scheduler.Component;
using DotnetSpider.Selector;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace DotnetSpider.Sample.samples;

public class CnBlogsSpider(
    IOptions<SpiderOptions> options,
    DependenceServices services,
    ILogger<Spider> logger)
    : Spider(options, services, logger)
{
    public static async Task RunAsync()
    {
        var builder = Builder.CreateDefaultBuilder<CnBlogsSpider>(x =>
        {
            x.Speed = 2;
        });
        builder.UseSerilog();
        builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
        await builder.Build().RunAsync();
    }

    protected override async Task InitializeAsync(CancellationToken stoppingToken = default)
    {
        AddDataFlow<ListNewsParser>();
        AddDataFlow<NewsParser>();
        var request = new Request("https://news.cnblogs.com/n/page/1") { };
        request.Headers.UserAgent = "";
        await AddRequestsAsync(request);
    }

    protected override SpiderId GenerateSpiderId()
    {
        return new(ObjectId.CreateId().ToString(), "博客园");
    }

    protected class ListNewsParser : DataParser
    {
        public override Task InitializeAsync()
        {
            AddRequiredValidator("news\\.cnblogs\\.com/n/page");
            // if you want to collect every pages
            AddFollowRequestQuerier(Selectors.XPath(".//div[@class='pager']"));
            return Task.CompletedTask;
        }

        protected override Task ParseAsync(DataFlowContext context)
        {
            var newsList = context.Selectable.SelectList(Selectors.XPath(".//div[@class='news_block']"));
            foreach (var news in newsList)
            {
                var title = news.Select(Selectors.XPath(".//h2[@class='news_entry']"))?.Value;
                var url = news.Select(Selectors.XPath(".//h2[@class='news_entry']/a/@href"))?.Value;
                var summary = news.Select(Selectors.XPath(".//div[@class='entry_summary']"))?.Value;
                var views = news.Select(Selectors.XPath(".//span[@class='view']"))?.Value.Replace(" 人浏览", "");

                if (!string.IsNullOrWhiteSpace(url))
                {
                    var request = context.CreateNewRequest(new Uri(url));
                    request.Properties.Add("title", title);
                    request.Properties.Add("url", url);
                    request.Properties.Add("summary", summary);
                    request.Properties.Add("views", views);

                    context.AddFollowRequests(request);
                }
            }

            return Task.CompletedTask;
        }
    }

    protected class NewsParser : DataParser
    {
        public override Task InitializeAsync()
        {
            AddRequiredValidator("news\\.cnblogs\\.com/n/\\d+");
            return Task.CompletedTask;
        }

        protected override Task ParseAsync(DataFlowContext context)
        {
            var typeName = typeof(News).FullName;
            var url = context.Request.RequestUri.ToString();
            var title = context.Request.Properties["title"]?.ToString()?.Trim();
            var summary = context.Request.Properties["summary"]?.ToString()?.Trim();
            var views = int.Parse(context.Request.Properties["views"]?.ToString()?.Trim() ?? "0");
            var content = context.Selectable.Select(Selectors.XPath(".//div[@id='news_body']"))?.Value
                ?.Trim();
            context.AddData(typeName,
                new News
                {
                    Url = url,
                    Title = title,
                    Summary = summary,
                    Views = views,
                    Content = content
                });

            return Task.CompletedTask;
        }
    }

    protected class News
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Summary { get; set; }
        public int Views { get; set; }
        public string Content { get; set; }
    }
}
