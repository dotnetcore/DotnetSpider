# DotnetSpider

免责申明：本框架是为了帮助开发人员简化开发流程、提高开发效率，请勿使用此框架做任何违法国家法律的事情，使用者所做任何事情也与本框架的作者无关。

[![Build Status](https://dev.azure.com/zlzforever/DotnetSpider/_apis/build/status/dotnetcore.DotnetSpider?branchName=master)](https://dev.azure.com/zlzforever/DotnetSpider/_build/latest?definitionId=3&branchName=master)
[![NuGet](https://img.shields.io/nuget/vpre/DotnetSpider.svg)](https://www.nuget.org/packages/DotnetSpider)
[![Member project of .NET Core Community](https://img.shields.io/badge/member%20project%20of-NCC-9e20c9.svg)](https://github.com/dotnetcore)
[![GitHub license](https://img.shields.io/github/license/dotnetcore/DotnetSpider.svg)](https://github.com/dotnetcore/DotnetSpider/blob/master/LICENSE.txt)

DotnetSpider, a .NET Standard web crawling library. It is lightweight, efficient and fast high-level web crawling & scraping framework.

If you want get latest beta packages, you should add the myget feed:

```
<add key="myget.org" value="https://www.myget.org/F/zlzforever/api/v3/index.json" protocolVersion="3" />
```

### DESIGN

![DESIGN IMAGE](https://github.com/dotnetcore/DotnetSpider/blob/master/images/%E6%95%B0%E6%8D%AE%E9%87%87%E9%9B%86%E7%B3%BB%E7%BB%9F.png?raw=true)

### DEVELOP ENVIROMENT

1. Visual Studio 2017 (15.3 or later) or Jetbrains Rider
2. [.NET Core 2.2 or later](https://www.microsoft.com/net/download/windows)
3. Docker
4. MySql

        docker run --name mysql -d -p 3306:3306 --restart always -e MYSQL_ROOT_PASSWORD=1qazZAQ! mysql:5.7

5. Redis (option)

        docker run --name redis -d -p 6379:6379 --restart always redis

6. SqlServer

        docker run --name sqlserver -d -p 1433:1433 --restart always  -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=1qazZAQ!' mcr.microsoft.com/mssql/server:2017-latest

8. PostgreSQL (option)

        docker run --name postgres -d  -p 5432:5432 --restart always -e POSTGRES_PASSWORD=1qazZAQ! postgres

9. MongoDb  (option)

        docker run --name mongo -d -p 27017:27017 --restart always mongo

10. RabbitMQ

        docker run -d --restart always --name rabbimq -p 4369:4369 -p 5671-5672:5671-5672 -p 25672:25672 -p 15671-15672:15671-15672 \
               -e RABBITMQ_DEFAULT_USER=user -e RABBITMQ_DEFAULT_PASS=password \
               rabbitmq:3-management

11. Docker remote api for mac

        docker run -d  --restart always --name socat -v /var/run/docker.sock:/var/run/docker.sock -p 2376:2375 bobrik/socat TCP4-LISTEN:2375,fork,reuseaddr UNIX-CONNECT:/var/run/docker.sock

12. HBase

        docker run -d --restart always --name hbase -p 20550:8080 -p 8085:8085 -p 9090:9090 -p 9095:9095 -p 16010:16010 dajobe/hbase

### MORE DOCUMENTS

https://github.com/dotnetcore/DotnetSpider/wiki

### SAMPLES

    Please see the Project DotnetSpider.Sample in the solution.

### BASE USAGE

[Base usage Codes](https://github.com/zlzforever/DotnetSpider/blob/master/src/DotnetSpider.Sample/samples/BaseUsage.cs)

### ADDITIONAL USAGE: Configurable Entity Spider

[View complete Codes](https://github.com/zlzforever/DotnetSpider/blob/master/src/DotnetSpider.Sample/samples/EntitySpider.cs)

		public class EntitySpider : Spider
		{
			public static async Task RunAsync()
			{
				var builder = Builder.CreateDefaultBuilder<EntitySpider>();
				builder.UseSerilog();
				builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
				await builder.Build().RunAsync();
			}

			public EntitySpider(IOptions<SpiderOptions> options, SpiderServices services, ILogger<Spider> logger) : base(
				options, services, logger)
			{
			}

			protected override async Task InitializeAsync(CancellationToken stoppingToken)
			{
				AddDataFlow(new DataParser<CnblogsEntry>());
				AddDataFlow(GetDefaultStorage());
				await AddRequestsAsync(
					new Request("https://news.cnblogs.com/n/page/1/", new Dictionary<string, string> {{"网站", "博客园"}}),
					new Request("https://news.cnblogs.com/n/page/2/", new Dictionary<string, string> {{"网站", "博客园"}}));
			}

			protected override (string Id, string Name) GetIdAndName()
			{
				return (ObjectId.NewId.ToString(), "博客园");
			}

			[Schema("cnblogs", "news")]
			[EntitySelector(Expression = ".//div[@class='news_block']", Type = SelectorType.XPath)]
			[GlobalValueSelector(Expression = ".//a[@class='current']", Name = "类别", Type = SelectorType.XPath)]
			[FollowRequestSelector(XPaths = new[] {"//div[@class='pager']"})]
			public class CnblogsEntry : EntityBase<CnblogsEntry>
			{
				protected override void Configure()
				{
					HasIndex(x => x.Title);
					HasIndex(x => new {x.WebSite, x.Guid}, true);
				}

				public int Id { get; set; }

				[Required]
				[StringLength(200)]
				[ValueSelector(Expression = "类别", Type = SelectorType.Environment)]
				public string Category { get; set; }

				[Required]
				[StringLength(200)]
				[ValueSelector(Expression = "网站", Type = SelectorType.Environment)]
				public string WebSite { get; set; }

				[StringLength(200)]
				[ValueSelector(Expression = "//title")]
				[ReplaceFormatter(NewValue = "", OldValue = " - 博客园")]
				public string Title { get; set; }

				[StringLength(40)]
				[ValueSelector(Expression = "GUID", Type = SelectorType.Environment)]
				public string Guid { get; set; }

				[ValueSelector(Expression = ".//h2[@class='news_entry']/a")]
				public string News { get; set; }

				[ValueSelector(Expression = ".//h2[@class='news_entry']/a/@href")]
				public string Url { get; set; }

				[ValueSelector(Expression = ".//div[@class='entry_summary']")]
				public string PlainText { get; set; }

				[ValueSelector(Expression = "DATETIME", Type = SelectorType.Environment)]
				public DateTime CreationTime { get; set; }
			}
		}

#### Distributed spider


[Read this document](https://github.com/dotnetcore/DotnetSpider/wiki/3-Distributed-Spider)

#### Puppeteer downloader

Coming soon

### NOTICE

#### when you use redis scheduler, please update your redis config:

    timeout 0
    tcp-keepalive 60

 ### Dependencies

| Package | License |
| --- | --- |
| Bert.RateLimiters | Apache 2.0 |
 | MessagePack  |  MIT   |
 | Newtonsoft.Json  |  MIT   |
 | Dapper  |  Apache 2.0   |
 | HtmlAgilityPack  |  MIT   |
 | ZCJ.HashedWheelTimer  |  MIT   |
 | murmurhash  |  Apache 2.0   |
 | Serilog.AspNetCore  |  Apache 2.0   |
 | Serilog.Sinks.Console  |  Apache 2.0   |
 | Serilog.Sinks.RollingFile  |  Apache 2.0   |
 | Serilog.Sinks.PeriodicBatching  |  Apache 2.0   |
 | MongoDB.Driver  |  Apache 2.0   |
 | MySqlConnector  |  MIT   |
 | AutoMapper.Extensions.Microsoft.DependencyInjection  | MIT   |
 | Docker.DotNet  |  MIT   |
 | BuildBundlerMinifier  |  Apache 2.0   |
 | Pomelo.EntityFrameworkCore.MySql  |  MIT   |
 | Quartz.AspNetCore  |  Apache 2.0    |
 | Quartz.AspNetCore.MySqlConnector  | Apache 2.0  |
 | Npgsql  |  PostgreSQL License   |
 | RabbitMQ.Client  |  Apache 2.0   |
 | Polly  | BSD 3-C   |

### Buy me a coffee

![](https://github.com/zlzforever/ClickHouseMigrator/raw/master/images/alipay.jpeg)

### AREAS FOR IMPROVEMENTS

QQ Group: 477731655
Email: zlzforever@163.com
