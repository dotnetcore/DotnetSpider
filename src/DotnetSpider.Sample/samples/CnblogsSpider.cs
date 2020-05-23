using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Http;
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
			builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
			await builder.Build().RunAsync();
		}

		public CnblogsSpider(IOptions<SpiderOptions> options,
			SpiderServices services,
			ILogger<Spider> logger) : base(
			options, services, logger)
		{
		}

		protected override async Task InitializeAsync(CancellationToken stoppingToken)
		{
			AddDataFlow(new ListNewsParser());
			AddDataFlow(new NewsParser());
			AddDataFlow(new MyConsoleStorage());
			await AddRequestsAsync(new Request("https://news.cnblogs.com/n/page/1/"));
		}

		protected override (string Id, string Name) GetIdAndName()
		{
			return (Guid.NewGuid().ToString(), "cnblogs");
		}

		protected class MyConsoleStorage : StorageBase
		{
			protected override Task StoreAsync(DataContext context)
			{
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
			public ListNewsParser()
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
			}

			protected override Task Parse(DataContext context)
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
						var request = context.CreateNewRequest(url);
						request.SetProperty("title", title);
						request.SetProperty("url", url);
						request.SetProperty("summary", summary);
						request.SetProperty("views", views);

						context.AddFollowRequests(request);
					}
				}

				return Task.CompletedTask;
			}
		}

		protected class NewsParser : DataParser
		{
			public NewsParser()
			{
				AddRequiredValidator("news\\.cnblogs\\.com/n/\\d+");
			}

			protected override Task Parse(DataContext context)
			{
				var typeName = typeof(News).FullName;
				context.AddData(typeName,
					new News
					{
						Url = context.Request.RequestUri.ToString(),
						Title = context.Request.Properties["title"]?.Trim(),
						Summary = context.Request.Properties["summary"]?.Trim(),
						Views = int.Parse(context.Request.Properties["views"]),
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
