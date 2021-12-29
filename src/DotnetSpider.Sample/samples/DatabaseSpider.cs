using System.Threading;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.DataFlow;
using DotnetSpider.Downloader;
using DotnetSpider.Http;
using DotnetSpider.Scheduler;
using DotnetSpider.Scheduler.Component;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Serilog;

namespace DotnetSpider.Sample.samples
{
	public class DatabaseSpider : CnblogsSpider
	{
		public static new async Task RunAsync()
		{
			var builder = Builder.CreateDefaultBuilder<DatabaseSpider>();
			builder.UseSerilog();
			builder.UseDownloader<HttpClientDownloader>();
			builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
			await builder.Build().RunAsync();
		}

		public DatabaseSpider(IOptions<SpiderOptions> options, DependenceServices services,
			ILogger<Spider> logger) : base(
			options, services, logger)
		{
		}

		protected override async Task InitializeAsync(CancellationToken stoppingToken = default)
		{
			AddDataFlow(new ListNewsParser());
			AddDataFlow(new NewsParser());
			AddDataFlow(new MyStorage());
			await AddRequestsAsync(new Request("https://news.cnblogs.com/n/page/1/"));
		}

		class MyStorage : DataFlowBase
		{
			public override async Task InitializeAsync()
			{
				await using var conn =
					new MySqlConnection(
						"Database='mysql';Data Source=localhost;password=1qazZAQ!;User ID=root;Port=3306;");
				await conn.ExecuteAsync("create database if not exists cnblogs2;");
				await conn.ExecuteAsync($@"
create table if not exists cnblogs2.news2
(
    id       int auto_increment
    primary key,
    title    varchar(500)      not null,
    url      varchar(500)      not null,
    summary  varchar(1000)     null,
    views    int               null,
    content  varchar(2000)     null
);
");
			}

			public override async Task HandleAsync(DataFlowContext context)
			{
				if (IsNullOrEmpty(context))
				{
					Logger.LogWarning("数据流上下文不包含解析结果");
					return;
				}

				var typeName = typeof(News).FullName;
				var data = (News)context.GetData(typeName);
				if (data != null)
				{
					await using var conn =
						new MySqlConnection(
							"Database='mysql';Data Source=localhost;password=1qazZAQ!;User ID=root;Port=3306;");
					await conn.ExecuteAsync(
						$"INSERT IGNORE INTO cnblogs2.news2 (title, url, summary, views, content) VALUES (@Title, @Url, @Summary, @Views, @Content);",
						data);
				}
			}
		}
	}
}
