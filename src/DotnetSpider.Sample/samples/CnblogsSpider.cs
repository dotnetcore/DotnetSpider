using System;
using System.Text.RegularExpressions;
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

namespace DotnetSpider.Sample.samples
{
	public class CnblogsSpider : Spider
	{
		public static async Task RunAsync()
		{
			var builder = Builder.CreateDefaultBuilder<CnblogsSpider>();
			builder.UseSerilog();
			builder.UseDownloader<HttpClientDownloader>();
			builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();

			await builder.Build().RunAsync();
		}

		public CnblogsSpider(IOptions<SpiderOptions> options,
			DependenceServices services,
			ILogger<Spider> logger) : base(
			options, services, logger)
		{
		}

		protected override async Task InitializeAsync(CancellationToken stoppingToken = default)
		{
			AddDataFlow(new ListNewsParser());
			AddDataFlow(new NewsParser());
			AddDataFlow(new MyConsoleStorage());
			await AddRequestsAsync(new Request("https://news.cnblogs.com/n/page/1/"));
		}

		protected override SpiderId GenerateSpiderId()
		{
			return new(ObjectId.CreateId().ToString(), "博客园");
		}

		protected class MyConsoleStorage : DataFlowBase
		{
			public override Task InitializeAsync()
			{
				return Task.CompletedTask;
			}

			public override Task HandleAsync(DataFlowContext context)
			{
				if (IsNullOrEmpty(context))
				{
					Logger.LogWarning("数据流上下文不包含解析结果");
					return Task.CompletedTask;
				}

				var typeName = typeof(News).FullName;
				var data = context.GetData(typeName);
				if (data is News news)
				{
					Console.WriteLine($"URL: {news.Url}, TITLE: {news.Title}, VIEWS: {news.Views}");
				}

				return Task.CompletedTask;
			}
		}

		protected class ListNewsParser : DataParser
		{
			public override Task InitializeAsync()
			{
				// AddRequiredValidator("news\\.cnblogs\\.com/n/page");
				AddRequiredValidator((request =>
				{
					var host = request.RequestUri.Host;
					var regex = host + "/$";
					return Regex.IsMatch(request.RequestUri.ToString(), regex);
				}));
				// if you want to collect every pages
				// AddFollowRequestQuerier(Selectors.XPath(".//div[@class='pager']"));
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
				context.AddData(typeName,
					new News
					{
						Url = context.Request.RequestUri.ToString(),
						Title = context.Request.Properties["title"]?.ToString()?.Trim(),
						Summary = context.Request.Properties["summary"]?.ToString()?.Trim(),
						Views = int.Parse(context.Request.Properties["views"]?.ToString()?.Trim() ?? "0"),
						Content = context.Selectable.Select(Selectors.XPath(".//div[@id='news_body']")).Value
							?.Trim()
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
}
