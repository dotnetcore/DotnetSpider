# DotnetSpider
=================

This is a cross platfrom, high performance, light weight cralwer developed by C#.

### DESIGN

![demo](http://images2015.cnblogs.com/blog/40347/201605/40347-20160511101118155-1794710718.jpg)

### DEVELOP ENVIROMENT
- Visual Studio 2015 update 3 or later
- [Visual Studio 2015 Tools (Preview 2)](https://www.microsoft.com/net/download/core)
- [.NET Core SDK](https://www.microsoft.com/net/download/core)

### OPTIONAL ENVIROMENT

- If you want to save data to mysql. [Download MySql](http://dev.mysql.com/get/Downloads/MySQLInstaller/mysql-installer-community-5.7.14.0.msi)
- If you want to run distributed crawler. [Download Redis for windows](https://github.com/MSOpenTech/redis/releases)
- MSSQL.

### SAMPLES

	Please see the Projet DotnetSpider.Sample in the solution.

### BASE USAGE

[Base usage Codes](https://github.com/zlzforever/DotnetSpider/blob/master/src/DotnetSpider.Sample/BaseUsage.cs)

##### Crawler pages traversal

		public static void CrawlerPagesTraversal()
		{
			// 定义要采集的 Site 对象, 可以设置 Header、Cookie、代理等
			var site = new Site { EncodingName = "UTF-8", RemoveOutboundLinks = true };

			// Set start/seed url
			site.AddStartUrl("http://www.cnblogs.com/");

			Spider spider = Spider.Create(site,

				// crawler identity
				"cnblogs_" + DateTime.Now.ToString("yyyyMMddhhmmss"),

				// use memoery queue scheduler
				new QueueDuplicateRemovedScheduler(),

				// default page processor will save whole html, and extract urls to target urls via regex
				new DefaultPageProcessor("cnblogs\\.com"))

				// save crawler result to file in the folder: \{running directory}\data\{crawler identity}\{guid}.dsd
				.AddPipeline(new FilePipeline())

				// dowload html by http client
				.SetDownloader(new HttpClientDownloader())

				// 4 threads 4线程
				.SetThreadNum(4);

			// traversal deep 遍历深度
			spider.Deep = 3;

			// stop crawler if it can't get url from the scheduler after 30000 ms 当爬虫连续30秒无法从调度中心取得需要采集的链接时结束.
			spider.EmptySleepTime = 30000;

			// start crawler 启动爬虫
			spider.Run();
		}

##### Custmize processor and pipeline

		public static void CustmizeProcessorAndPipeline()
		{
			// Config encoding, header, cookie, proxy etc... 定义采集的 Site 对象, 设置 Header、Cookie、代理等
			var site = new Site { EncodingName = "UTF-8", RemoveOutboundLinks = true };
			for (int i = 1; i < 5; ++i)
			{
				// Add start/feed urls. 添加初始采集链接
				site.AddStartUrl("http://" + $"www.youku.com/v_olist/c_97_g__a__sg__mt__lg__q__s_1_r_0_u_0_pt_0_av_0_ag_0_sg__pr__h__d_1_p_{i}.html");
			}

			Spider spider = Spider.Create(site,

				// use memoery queue scheduler. 使用内存调度
				new QueueDuplicateRemovedScheduler(),

				// use custmize processor for youku 为优酷自定义的 Processor
				new YoukuPageProcessor())

				// use custmize pipeline for youku 为优酷自定义的 Pipeline
				.AddPipeline(new YoukuPipeline())

				// dowload html by http client
				.SetDownloader(new HttpClientDownloader())

				// 1 thread
				.SetThreadNum(1);

			spider.EmptySleepTime = 3000;

			// Start crawler 启动爬虫
			spider.Run();
		}

		public class YoukuPipeline : BasePipeline
		{
			private static long count = 0;

			public override void Process(ResultItems resultItems)
			{
				foreach (YoukuVideo entry in resultItems.Results["VideoResult"])
				{
					count++;
					Console.WriteLine($"[YoukuVideo {count}] {entry.Name}");
				}

				// Other actions like save data to DB. 可以自由实现插入数据库或保存到文件
			}
		}

		public class YoukuPageProcessor : BasePageProcessor
		{
			protected override void Handle(Page page)
			{
				// 利用 Selectable 查询并构造自己想要的数据对象
				var totalVideoElements = page.Selectable.SelectList(Selectors.XPath("//div[@class='yk-pack pack-film']")).Nodes();
				List<YoukuVideo> results = new List<YoukuVideo>();
				foreach (var videoElement in totalVideoElements)
				{
					var video = new YoukuVideo();
					video.Name = videoElement.Select(Selectors.XPath(".//img[@class='quic']/@alt")).GetValue();
					results.Add(video);
				}
				
				// Save data object by key. 以自定义KEY存入page对象中供Pipeline调用
				page.AddResultItem("VideoResult", results);

				// Add target requests to scheduler. 解析需要采集的URL
				foreach (var url in page.Selectable.SelectList(Selectors.XPath("//ul[@class='yk-pages']")).Links().Nodes())
				{
					page.AddTargetRequest(new Request(url.GetValue(), null));
				}
			}
		}

		public class YoukuVideo
		{
			public string Name { get; set; }
		}
	
### ADDITIONAL USAGE

#### Configurable Entity Spider

[View compelte Codes](https://github.com/zlzforever/DotnetSpider/blob/master/src/DotnetSpider.Sample/JdSkuSampleSpider.cs)

	public class JdSkuSampleSpider : EntitySpiderBuilder
	{
		protected override EntitySpider GetEntitySpider()
		{
			EntitySpider context = new EntitySpider(new Site
			{
				//HttpProxyPool = new HttpProxyPool(new KuaidailiProxySupplier("快代理API"))
			});
			context.SetThreadNum(1);
			context.SetIdentity("JD_sku_store_test_" + DateTime.Now.ToString("yyyy_MM_dd_hhmmss"));
			// save data to mysql.
			context.AddEntityPipeline(new MySqlEntityPipeline("Database='test';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"));
			// dowload html by http client
			context.SetDownloader(new HttpClientDownloader())
			context.AddStartUrl("http://list.jd.com/list.html?cat=9987,653,655&page=2&JL=6_0_0&ms=5#J_main", new Dictionary<string, object> { { "name", "手机" }, { "cat3", "655" } });
			context.AddEntityType(typeof(Product));
			return context;
		}

		[Schema("test", "sku", TableSuffix.Today)]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		[Indexes(Index = new[] { "category" }, Unique = new[] { "category,sku", "sku" })]
		[TargetUrlsSelector(XPaths = new[] { "//span[@class=\"p-num\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Product : ISpiderEntity
		{
			[StoredAs("sku", DataType.String, 25)]
			[PropertySelector(Expression = "./@data-sku")]
			public string Sku { get; set; }

			[StoredAs("category", DataType.String, 20)]
			[PropertySelector(Expression = "name", Type = SelectorType.Enviroment)]
			public string CategoryName { get; set; }

			[StoredAs("cat3", DataType.String, 20)]
			[PropertySelector(Expression = "cat3", Type = SelectorType.Enviroment)]
			public int CategoryId { get; set; }

			[StoredAs("url", DataType.Text)]
			[PropertySelector(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[StoredAs("commentscount", DataType.String, 32)]
			[PropertySelector(Expression = "./div[5]/strong/a")]
			public long CommentsCount { get; set; }

			[StoredAs("shopname", DataType.String, 100)]
			[PropertySelector(Expression = ".//div[@class='p-shop']/@data-shop_name")]
			public string ShopName { get; set; }

			[StoredAs("name", DataType.String, 50)]
			[PropertySelector(Expression = ".//div[@class='p-name']/a/em")]
			public string Name { get; set; }

			[StoredAs("venderid", DataType.String, 25)]
			[PropertySelector(Expression = "./@venderid")]
			public string VenderId { get; set; }

			[StoredAs("jdzy_shop_id", DataType.String, 25)]
			[PropertySelector(Expression = "./@jdzy_shop_id")]
			public string JdzyShopId { get; set; }

			[StoredAs("run_id", DataType.Date)]
			[PropertySelector(Expression = "Monday", Type = SelectorType.Enviroment)]
			public DateTime RunId { get; set; }

			[PropertySelector(Expression = "Now", Type = SelectorType.Enviroment)]
			[StoredAs("cdate", DataType.Time)]
			public DateTime CDate { get; set; }
		}
	}

	public static void Main()
	{
		JdSkuSampleSpider spider = new JdSkuSampleSpider();
		spider.Run();
	}

#### WebDriver Support

When you want to collect a page JS loaded, there is only one thing to do, set the downloader to WebDriverDownloader.

	context.SetDownloader(new WebDriverDownloader(Browser.Chrome));
	or
	spider.SetDownloader(new WebDriverDownloader(Browser.Chrome));

[See a complete sample](https://github.com/zlzforever/DotnetSpider/blob/master/src/DotnetSpider.Sample/JdSkuWebDriverSample.cs)

NOTE:

1. Make sure there is a  ChromeDriver.exe in bin forlder when you set Browser to Chrome. You can contain it to your project via NUGET manager: Chromium.ChromeDriver
2. Make sure you already add a *.webdriver Firefox profile when you set Browser to Firefox: https://support.mozilla.org/en-US/kb/profile-manager-create-and-remove-firefox-profiles
3. Make sure there is a PhantomJS.exe in bin folder when you set Browser to PhantomJS. You can contain it to your project via NUGET manager: PhantomJS

### Monitor

1. Set logAndStatusConnectString to correct mysql connect string in config.ini project DotnetSpider.Sample.
2. Update MySqlConnectString in appsettings.json in DotnetSpider.Enterpise project.

![monitor](https://raw.githubusercontent.com/zlzforever/DotnetSpider/master/images/1.png)

### Web Manager



### NOTICE

#### when you use redis scheduler, please update your redis config: 
	timeout 0 
	tcp-keepalive 60

### AREAS FOR IMPROVEMENTS

QQ Group: 477731655
Email: zlzforever@163.com