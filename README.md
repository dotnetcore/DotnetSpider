# DotnetSpider

[![Build Status](https://dev.azure.com/zlzforever/DotnetSpider/_apis/build/status/dotnetcore.DotnetSpider?branchName=master)](https://dev.azure.com/zlzforever/DotnetSpider/_build/latest?definitionId=3&branchName=master)
[![NuGet](https://img.shields.io/nuget/vpre/DotnetSpider.svg)](https://www.nuget.org/packages/DotnetSpider)
[![Member project of .NET Core Community](https://img.shields.io/badge/member%20project%20of-NCC-9e20c9.svg)](https://github.com/dotnetcore)
[![GitHub license](https://img.shields.io/github/license/dotnetcore/DotnetSpider.svg)](https://raw.githubusercontent.com/dotnetcore/DotnetSpider/master/LICENSE)

DotnetSpider, a .NET Standard web crawling library. It is lightweight, efficient and fast high-level web crawling & scraping framework

### DESIGN

```
                                  +----------------------+  +----------------------+      
                                  | Download Center      |  | Statistics Center    |    
+----------------------+          +----------^-----------+  +----------^-----------+  
| Downloader Agent 1   +----+                |                         |                               
+----------------------+    |                |                         |                
                            |     +----------v-------Message Queue-----v-----------+    +------------- Scheduler-------------------+
+----------------------+    |     |  +-------+       +----------+       +-------+  |    |  +-------+    +-------+    +----------+  |
| Downloader Agent 2   +----+<---->  | Local |       | RabbitMq |       | Kafka |  |    |  | Local |    | Redis |    | Database |  |
+----------------------+    |     |  +-------+       +----------+       +-------+  |    |  +-------+    +-------+    +----------+  |
                            |     +-----------------------^------------------------+    +-------------------^----------------------+   
+----------------------+    |                             |                                                 |
| Downloader Agent 3   +----+                             |                                                 |
+----------------------+          +-------Spider----------v--------------------------+                      |
                                  |    +-----------------+  +--------------------+   |                      |
                                  |    | SpeedController |  | RequestSupply      |   |                      |
                                  |    +-----------------+  +--------------------+   <----------------------+             
                                  |    +----------------------------+  +----------+  |                      |
                                  |    | Configure Request delegate |  | DataFlow |  |                      |
                                  |    +----------------------------+  +----------+  |                      |       
                                  +--------------------------------------------------+          +-----------v--------------+
                                                                                                |  MySql, SqlServer, etc   |
                                                                                                +-----------+--------------+
                                                                                                            |
                                                                                                            |
                                                                                                +-----------v--------------+
                                                                                                |        ClickHouse        |
                                                                                                +--------------------------+     
                                                                                                                        

``` 

### DEVELOP ENVIROMENT

- Visual Studio 2017 (15.3 or later)
- [.NET Core 2.2 or later](https://www.microsoft.com/net/download/windows)

### OPTIONAL ENVIROMENT

- MySql
- Redis [Download Redis for Windows](https://github.com/MSOpenTech/redis/releases)
- SqlServer
- PostgreSQL
- MongoDb

### MORE DOCUMENTS

https://github.com/dotnetcore/DotnetSpider/wiki

### SAMPLES

    Please see the Projet DotnetSpider.Sample in the solution.

### BASE USAGE

[Base usage Codes](https://github.com/zlzforever/DotnetSpider/blob/master/src/DotnetSpider.Sample/samples/BaseUsage.cs)

### ADDITIONAL USAGE: Configurable Entity Spider

[View complete Codes](https://github.com/zlzforever/DotnetSpider/blob/master/src/DotnetSpider.Sample/samples/EntitySpider.cs)

    public class EntitySpider : Spider
    {
        public static void Run()
        {
            var builder = new SpiderBuilder();
            builder.AddSerilog();
            builder.ConfigureAppConfiguration();
            builder.UseStandalone();
            builder.AddSpider<EntitySpider>();
            var provider = builder.Build();
            provider.Create<EntitySpider>().RunAsync();
        }

        protected override void Initialize()
        {
            NewGuidId();
            Scheduler = new QueueDistinctBfsScheduler();
            Speed = 1;
            Depth = 3;
            DownloaderSettings.Type = DownloaderType.HttpClient;
            AddDataFlow(new DataParser<BaiduSearchEntry>()).AddDataFlow(GetDefaultStorage());
            AddRequests(
                new Request("https://news.cnblogs.com/n/page/1/", new Dictionary<string, string> {{"网站", "博客园"}}),
                new Request("https://news.cnblogs.com/n/page/2/", new Dictionary<string, string> {{"网站", "博客园"}}));
        }

        [Schema("cnblogs", "cnblogs_entity_model")]
        [EntitySelector(Expression = ".//div[@class='news_block']", Type = SelectorType.XPath)]
        [ValueSelector(Expression = ".//a[@class='current']", Name = "类别", Type = SelectorType.XPath)]
        class BaiduSearchEntry : EntityBase<BaiduSearchEntry>
        {
            protected override void Configure()
            {
                HasIndex(x => x.Title);
                HasIndex(x => new {x.WebSite, x.Guid}, true);
            }

            public int Id { get; set; }

            [Required]
            [StringLength(200)]
            [ValueSelector(Expression = "类别", Type = SelectorType.Enviroment)]
            public string Category { get; set; }

            [Required]
            [StringLength(200)]
            [ValueSelector(Expression = "网站", Type = SelectorType.Enviroment)]
            public string WebSite { get; set; }

            [StringLength(200)]
            [ValueSelector(Expression = "//title")]
            [ReplaceFormatter(NewValue = "", OldValue = " - 博客园")]
            public string Title { get; set; }

            [StringLength(40)]
            [ValueSelector(Expression = "GUID", Type = SelectorType.Enviroment)]
            public string Guid { get; set; }

            [ValueSelector(Expression = ".//h2[@class='news_entry']/a")]
            public string News { get; set; }

            [ValueSelector(Expression = ".//h2[@class='news_entry']/a/@href")]
            public string Url { get; set; }

            [ValueSelector(Expression = ".//div[@class='entry_summary']", ValueOption = ValueOption.InnerText)]
            public string PlainText { get; set; }

            [ValueSelector(Expression = "DATETIME", Type = SelectorType.Enviroment)]
            public DateTime CreationTime { get; set; }
        }

        public EntitySpider(IMessageQueue mq, IStatisticsService statisticsService, ISpiderOptions options, ILogger<Spider> logger, IServiceProvider services) : base(mq, statisticsService, options, logger, services)
        {
        }
    }

#### Run via Startup

    Command: -s [spider type name] -i [id] -a [arg1,arg2...] -d [true/false] -n [name] -c [configuration file]

    1.  -s: Type name of spider for example: EntitySpider
    2.  -i: Set spider id
    3.  -a: Pass arguments to spider's Run method
    4.  -n: Set spider name
    5.  -c: Set config file path, for example you want to run with a customize config: -c app.my.config

#### WebDriver Support

When you want to collect a page JS loaded, there is only one thing to do, set the downloader to WebDriverDownloader.

    Downloader=new WebDriverDownloader(Browser.Chrome);

[See a complete sample](https://github.com/zlzforever/DotnetSpider/)

NOTE:

1.  Make sure the ChromeDriver.exe is in bin folder when use Chrome, install it to your project from NUGET: Chromium.ChromeDriver
2.  Make sure you already add a \*.webdriver Firefox profile when use Firefox: https://support.mozilla.org/en-US/kb/profile-manager-create-and-remove-firefox-profiles
3.  Make sure the PhantomJS.exe is in bin folder when use PhantomJS, install it to your project from NUGET: PhantomJS


### NOTICE

#### when you use redis scheduler, please update your redis config:

    timeout 0
    tcp-keepalive 60

### Buy me a coffee

![](https://github.com/zlzforever/DotnetSpiderPictures/raw/master/pay.png)

### AREAS FOR IMPROVEMENTS

QQ Group: 477731655
Email: zlzforever@163.com
