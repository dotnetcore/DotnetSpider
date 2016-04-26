# DotnetSpider
This is a cross platfrom spider develop by C#.

# Design
Spider need 3 parts to work: Scheduler, Downloader, Pipeline.

# Base use

		public static void Main()
		{
			HttpClientDownloader downloader = new HttpClientDownloader();

			Core.Spider spider = Core.Spider.Create(new MyPageProcessor(), new QueueDuplicateRemovedScheduler()).AddPipeline(new MyPipeline()).SetThreadNum(1);
			var site = new Site() { EncodingName = "UTF-8" };
			for (int i = 1; i < 5; ++i)
			{
				site.AddStartUrl("http://www.youku.com/v_olist/c_97_g__a__sg__mt__lg__q__s_1_r_0_u_0_pt_0_av_0_ag_0_sg__pr__h__d_1_p_1.html");
			}
			spider.Site = site;
			spider.Start();
		}

		private class MyPipeline : IPipeline
		{
			public void Process(ResultItems resultItems, ISpider spider)
			{
				foreach (YoukuVideo entry in resultItems.Results["VideoResult"])
				{
					Console.WriteLine($"{entry.Name}:{entry.Click}");
				}

				//May be you want to save to database
				// 
			}

			public void Dispose()
			{
			}
		}

		private class MyPageProcessor : IPageProcessor
		{
			public void Process(Page page)
			{
				var totalVideoElements = page.Selectable.SelectList(Selectors.XPath("//div[@class='yk-col3']")).Nodes();
				List<YoukuVideo> results = new List<YoukuVideo>();
				foreach (var videoElement in totalVideoElements)
				{
					var video = new YoukuVideo();
					video.Name = videoElement.Select(Selectors.XPath("/div[4]/div[1]/a")).Value;
					video.Click = int.Parse(videoElement.Select(Selectors.Css("p-num")).Value.ToString());
					results.Add(video);
				}
				page.AddResultItem("VideoResult", results);
			}

			public Site Site => new Site { SleepTime = 0 };
		}

		public class YoukuVideo
		{
			public string Name { get; set; }
			public string Click { get; set; }
		}
	
#Object Auto Extractor

###1. Add config file: app.conf to your project
    
	redisServer: your redis server
	redisPassword:your redis password

###2. Add spider contentx class

	public class JdSkuSpider : ISpiderContext
	{
		public SpiderContextBuilder GetBuilder()
		{
			Log.TaskId = "JD SKU Weekly";
			SpiderContext context = new SpiderContext
			{
				SpiderName = "JD SKU " + DateTimeUtils.MONDAY_RUN_ID,
				CachedSize = 1,
				ThreadNum = 8,
				Site = new Site
				{
					EncodingName = "UTF-8"
				},
				Scheduler = new RedisScheduler()
				{
					Host = "redis",
					Port = 6379,
					Password = ""
				},
				StartUrls=new Dictionary<string, Dictionary<string, object>> {
					{ "http://list.jd.com/list.html?cat=9987,653,655&page=1&go=0&JL=6_0_0&ms=5", new Dictionary<string, object> { { "name","手机" }, { "cat3","9987" } } },
				},
				Pipeline = new MysqlPipeline()
				{
					ConnectString = ""
				},
				Downloader = new HttpDownloader()
			};
			return new SpiderContextBuilder(context, typeof(Product));
		}

		[Schema("jd", "sku_v2", Suffix = TableSuffix.Monday)]
		[TargetUrl(new[] { @"page=[0-9]+" }, "//*[@id=\"J_bottomPage\"]")]
		[TypeExtractBy(Expression = "//div[contains(@class,'j-sku-item')]", Multi = true)]
		[Indexes(Primary = "sku")]
		public class Product : ISpiderEntity
		{
			private static readonly DateTime runId;

			static Product()
			{
				DateTime dt = DateTime.Now;
				runId = new DateTime(dt.Year, dt.Month, 1);
			}

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

			[StoredAs("commentscount", DataType.String, 20)]
			[PropertyExtractBy(Expression = "./div[@class='p-commit']/strong/a")]
			public long CommentsCount { get; set; }

			[StoredAs("shopname", DataType.String, 100)]
			[PropertyExtractBy(Expression = "./div[@class='p-shop hide']/span[1]/a[1]")]
			public string ShopName { get; set; }

			[StoredAs("name", DataType.String, 50)]
			[PropertyExtractBy(Expression = "./div[@class='p-name']/a/em")]
			public string Name { get; set; }

			[StoredAs("shopid", DataType.String, 25)]
			public string ShopId { get; set; }

			[StoredAs("venderid", DataType.String, 25)]
			[PropertyExtractBy(Expression = "./@venderid")]
			public string VenderId { get; set; }

			[StoredAs("jdzy_shop_id", DataType.String, 25)]
			[PropertyExtractBy(Expression = "./@jdzy_shop_id")]
			public string JdzyShopId { get; set; }

			[StoredAs("run_id", DataType.Date)]
			public string RunId => DateTimeUtils.MONDAY_RUN_ID;

			[StoredAs("cdate", DataType.Time)]
			public DateTime CDate => DateTime.Now;
		}
	}

