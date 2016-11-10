# DotnetSpider
=================

This is a cross platfrom, ligth spider develop by C#.

### DEVELOP ENVIROMENT
- Visual Studio 2015 or later
- Make sure installed 2 packages to try .NET CORE, [More details](https://www.microsoft.com/net/core#windows)
	1. [Visual studio 2015 update 3](https://go.microsoft.com/fwlink/?LinkId=691129)
	2. [.NET Core 1.0.0 - VS 2015 Tooling Preview 2](https://go.microsoft.com/fwlink/?LinkID=824849)

### DESIGN

![demo](http://images2015.cnblogs.com/blog/40347/201605/40347-20160511101118155-1794710718.jpg)

### TEST CASE ENVIROMENT

1. Install MySql in local and set account: root password: 1qazZAQ! [[Download MySql Community Server]](http://dev.mysql.com/get/Downloads/MySQLInstaller/mysql-installer-community-5.7.14.0.msi)
2. Install Redis in local without password [[Download Redis for Windows]](https://github.com/MSOpenTech/redis/releases)

![test](https://raw.githubusercontent.com/zlzforever/DotnetSpider/master/images/test.png)

### SAMPLE

	Please see the Projet: DotnetSpider.Sample, I will update follow spider's upgrade.

### BASE USAGE

Codes: https://github.com/zlzforever/DotnetSpider/blob/master/src/DotnetSpider.Sample/BaseUsage.cs

		static void Main(string[] args)
		{
			// 注入监控服务
			IocManager.AddSingleton<IMonitor, NLogMonitor>();

			// 定义要采集的 Site 对象, 可以设置 Header、Cookie、代理等
			var site = new Site { EncodingName = "UTF-8" };
			for (int i = 1; i < 5; ++i)
			{
				// 添加初始采集链接
				site.AddStartUrl("http://" + $"www.youku.com/v_olist/c_97_g__a__sg__mt__lg__q__s_1_r_0_u_0_pt_0_av_0_ag_0_sg__pr__h__d_1_p_{i}.html");
			}

			// 使用内存Scheduler、自定义PageProcessor、自定义Pipeline创建爬虫
			Spider spider = Spider.Create(site, new QueueDuplicateRemovedScheduler(), new MyPageProcessor()).AddPipeline(new MyPipeline()).SetThreadNum(1);
			spider.EmptySleepTime = 3000;
			// 注册爬虫到监控服务
			MonitorCenter.Register(spider);

			// 启动爬虫
			spider.Run();
			Console.Read();
		}

		private class MyPipeline : BasePipeline
		{
			private static long count = 0;

			public override void Process(ResultItems resultItems)
			{
				foreach (YoukuVideo entry in resultItems.Results["VideoResult"])
				{
					count++;
					Console.WriteLine($"[YoukuVideo {count}] {entry.Name}");
				}

				// 可以自由实现插入数据库或保存到文件
			}
		}

		private class MyPageProcessor : BasePageProcessor
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
				// 以自定义KEY存入page对象中供Pipeline调用
				page.AddResultItem("VideoResult", results);

				foreach (var url in page.Selectable.SelectList(Selectors.XPath("//ul[@class='yk-pages']")).Links().Nodes())
				{
					page.AddTargetRequest(new Request(url.GetValue(), 0, null));
				}
			}
		}

		public class YoukuVideo
		{
			public string Name { get; set; }
		}
	
### ADDITIONAL USAGE

#### Configurable Entity Spider

Codes: https://github.com/zlzforever/DotnetSpider/blob/master/src/DotnetSpider.Sample/JdSkuSampleSpider.cs

		public class JdSkuSampleSpider : EntitySpiderBuilder
		{
			protected override EntitySpider GetEntitySpider()
			{
				EntitySpider context = new EntitySpider(new Site());
				context.SetThreadNum(1);
				context.SetIdentity("JD_sku_store_test_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmss"));
				context.AddEntityPipeline(
					new MySqlEntityPipeline("Database='test';Data Source=MYSQLSERVER;User ID=root;Password=1qazZAQ!;Port=4306"));
				context.AddStartUrl("http://list.jd.com/list.html?cat=9987,653,655&page=2&JL=6_0_0&ms=5#J_main",
					new Dictionary<string, object> { { "name", "手机" }, { "cat3", "655" } });
				context.AddEntityType(typeof(Product));
				return context;
			}
	
			[Schema("test", "sku", TableSuffix.Today)]
			[TypeExtractBy(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]", Multi = true)]
			[Indexes(Index = new[] { "category" }, Unique = new[] { "category,sku", "sku" })]
			[TargetUrlsSelector(XPaths = new[] { "//span[@class=\"p-num\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
			public class Product : ISpiderEntity
			{
				[StoredAs("category", DataType.String, 20)]
				[PropertyExtractBy(Expression = "name", Type = ExtractType.Enviroment)]
				public string CategoryName { get; set; }
	
				[StoredAs("cat3", DataType.String, 20)]
				[PropertyExtractBy(Expression = "cat3", Type = ExtractType.Enviroment)]
				public int CategoryId { get; set; }
	
				[StoredAs("url", DataType.Text)]
				[PropertyExtractBy(Expression = "./div[1]/a/@href")]
				public string Url { get; set; }
	
				[StoredAs("sku", DataType.String, 25)]
				[PropertyExtractBy(Expression = "./@data-sku")]
				public string Sku { get; set; }
	
				[StoredAs("commentscount", DataType.String, 32)]
				[PropertyExtractBy(Expression = "./div[5]/strong/a")]
				public long CommentsCount { get; set; }
	
				[StoredAs("shopname", DataType.String, 100)]
				[PropertyExtractBy(Expression = ".//div[@class='p-shop']/@data-shop_name")]
				public string ShopName { get; set; }
	
				[StoredAs("name", DataType.String, 50)]
				[PropertyExtractBy(Expression = ".//div[@class='p-name']/a/em")]
				public string Name { get; set; }
	
				[StoredAs("venderid", DataType.String, 25)]
				[PropertyExtractBy(Expression = "./@venderid")]
				public string VenderId { get; set; }
	
				[StoredAs("jdzy_shop_id", DataType.String, 25)]
				[PropertyExtractBy(Expression = "./@jdzy_shop_id")]
				public string JdzyShopId { get; set; }
	
				[StoredAs("run_id", DataType.Date)]
				[PropertyExtractBy(Expression = "Monday", Type = ExtractType.Enviroment)]
				public DateTime RunId { get; set; }
	
				[PropertyExtractBy(Expression = "Now", Type = ExtractType.Enviroment)]
				[StoredAs("cdate", DataType.Time)]
				public DateTime CDate { get; set; }
			}
		}

		public static void Main()
		{
			IocExtension.ServiceCollection.AddSingleton<IMonitorService, NLogMonitor>();
		
			JdSkuSampleSpider spider = new JdSkuSampleSpider();
			spider.Run();
		}

#### WebDriver Support

When you want to collect a page JS loaded, there is only one thing you need to do is set the downloader to WebDriverDownloader.	

	context.SetDownloader(new WebDriverDownloader(Browser.Chrome));

See the complete sample https://github.com/zlzforever/DotnetSpider/blob/master/src/DotnetSpider.Sample/JdSkuWebDriverSample.cs

NOTE:

1. Make sure there is a  ChromeDriver.exe in bin forlder when you set Browser to Chrome. You can contain it to your project via NUGET manager: Chromium.ChromeDriver
2. Make sure you already add a *.webdriver Firefox profile when you set Browser to Firefox: https://support.mozilla.org/en-US/kb/profile-manager-create-and-remove-firefox-profiles
3. Make sure there is a PhantomJS.exe in bin folder when you set Browser to PhantomJS. You can contain it to your project via NUGET manager: PhantomJS

### Monitor

1. Like the project DotnetSpider.Sample, make sure the logAndStatusConnectString is correct in config.ini
2. Update MySqlConnectString in appsettings.json in DotnetSpider.Portal project.
3. Run you spider then you can watch the status on the website

![monitor](https://raw.githubusercontent.com/zlzforever/DotnetSpider/master/images/1.png)

### NOTICE

#### when you use redis scheduler, please update your redis config: 
	timeout 0 
	tcp-keepalive 60


### UPDATES

1.0.0.0-PRE

### AREAS FOR IMPROVEMENTS

QQ: 477731655
EMail: zlzforever@163.com