# DotnetSpider
[![Travis branch](https://travis-ci.org/dotnetcore/DotnetSpider.svg?branch=master)](https://travis-ci.org/dotnetcore/DotnetSpider)
[![NuGet](https://img.shields.io/nuget/v/DotnetSpider2.Extension.svg)](https://www.nuget.org/packages/DotnetSpider2.Extension)
[![Member project of .NET China Foundation](https://img.shields.io/badge/member_project_of-.NET_CHINA-red.svg?style=flat&colorB=9E20C8)](https://github.com/dotnetcore)
[![GitHub license](https://img.shields.io/aur/license/yaourt.svg)](https://raw.githubusercontent.com/dotnetcore/DotnetSpider/master/LICENSE)

DotnetSpider, a .NET Standard web crawling library similar to WebMagic and Scrapy. It is a lightweight ,efficient and fast high-level web crawling & scraping framework for .NET

### DESIGN

![DESIGN](https://github.com/dotnetcore/DotnetSpider/raw/master/images/DESIGN.jpg)

### DEVELOP ENVIROMENT
- Visual Studio 2017(15.3 or later)
- [.NET Core 2.0 or later](https://www.microsoft.com/net/download/windows)

### OPTIONAL ENVIROMENT

- Storage data to mysql. [Download MySql](https://dev.mysql.com/downloads/mysql/) 
	
		grant all on *.* to 'root'@'localhost' IDENTIFIED BY '' with grant option;
	
		flush privileges;

- Run distributed crawler. [Download Redis for windows](https://github.com/MSOpenTech/redis/releases)
- SqlServer.
- PostgreSQL.
- MongoDb
- Cassandra

### MORE DOCUMENTS

https://github.com/dotnetcore/DotnetSpider/wiki

### SAMPLES

	Please see the Projet DotnetSpider.Sample in the solution.

### BASE USAGE

[Base usage Codes](https://github.com/zlzforever/DotnetSpider/blob/master/src/DotnetSpider.Sample/BaseUsage.cs)

### ADDITIONAL USAGE: Configurable Entity Spider

[View compelte Codes](https://github.com/zlzforever/DotnetSpider/blob/master/src/DotnetSpider.Sample/JdSkuSampleSpider.cs)

	[TaskName("JdSkuSampleSpider")]
	public class JdSkuSampleSpider : EntitySpider
	{
		public JdSkuSampleSpider() : base("JdSkuSample", new Site
		{
		})
		{
		}

		protected override void MyInit(params string[] arguments)
		{
			Identity = Identity ?? "JD SKU SAMPLE";
			// storage data to mysql, default is mysql entity pipeline, so you can comment this line. Don't miss sslmode.
			AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=;Port=3306;SslMode=None;"));
			AddStartUrl("http://list.jd.com/list.html?cat=9987,653,655&page=2&JL=6_0_0&ms=5#J_main", new Dictionary<string, object> { { "name", "手机" }, { "cat3", "655" } });
			AddEntityType<Product>();
		}
	}

	[EntityTable("test", "jd_sku", EntityTable.Monday, Indexs = new[] { "Category" }, Uniques = new[] { "Category,Sku", "Sku" })]
	[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
	[TargetUrlsSelector(XPaths = new[] { "//span[@class=\"p-num\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
	public class Product : SpiderEntity
	{
		[PropertyDefine(Expression = "./@data-sku", Length = 100)]
		public string Sku { get; set; }

		[PropertyDefine(Expression = "name", Type = SelectorType.Enviroment, Length = 100)]
		public string Category { get; set; }

		[PropertyDefine(Expression = "cat3", Type = SelectorType.Enviroment)]
		public int CategoryId { get; set; }

		[PropertyDefine(Expression = "./div[1]/a/@href")]
		public string Url { get; set; }

		[PropertyDefine(Expression = "./div[5]/strong/a")]
		public long CommentsCount { get; set; }

		[PropertyDefine(Expression = ".//div[@class='p-shop']/@data-shop_name", Length = 100)]
		public string ShopName { get; set; }

		[PropertyDefine(Expression = "0", Type = SelectorType.Enviroment)]
		public int ShopId { get; set; }

		[PropertyDefine(Expression = ".//div[@class='p-name']/a/em", Length = 100)]
		public string Name { get; set; }

		[PropertyDefine(Expression = "./@venderid", Length = 100)]
		public string VenderId { get; set; }

		[PropertyDefine(Expression = "./@jdzy_shop_id", Length = 100)]
		public string JdzyShopId { get; set; }

		[PropertyDefine(Expression = "Monday", Type = SelectorType.Enviroment)]
		public DateTime RunId { get; set; }
	}

	public static void Main()
	{
		Startup.Run(new string[] { "-s:JdSkuSampleSpider", "-tid:JdSkuSampleSpider", "-i:guid" });
	}

#### Run via Startup

	Command: -s:[spider type name | TaskName attribute] -i:[identity] -a:[arg1,arg2...] -tid:[taskId] -n:[name] -c:[configuration file path]

1. -s: Type name of spider or TaskNameAttribute for example: DotnetSpider.Sample.BaiduSearchSpiderl
2. -i: Set identity.
3. -a: Pass arguments to spider's Run method.
4. -tid: Set task id.
5. -n: Set name.
6. -c: Set config file path, for example you want to run with a customize config: -e:app.my.config

#### WebDriver Support

When you want to collect a page JS loaded, there is only one thing to do, set the downloader to WebDriverDownloader.

	Downloader=new WebDriverDownloader(Browser.Chrome);

[See a complete sample](https://github.com/zlzforever/DotnetSpider/blob/master/src/DotnetSpider.Sample/JdSkuWebDriverSample.cs)

NOTE:

1. Make sure there is a  ChromeDriver.exe in bin forlder when you try to use Chrome. You can contain it to your project via NUGET manager: Chromium.ChromeDriver
2. Make sure you already add a *.webdriver Firefox profile when you try to use Firefox: https://support.mozilla.org/en-US/kb/profile-manager-create-and-remove-firefox-profiles
3. Make sure there is a PhantomJS.exe in bin folder when you try to use PhantomJS. You can contain it to your project via NUGET manager: PhantomJS

### Storage log and status to database

1. Set SystemConnection in app.config
2. Update nlog.config like https://github.com/dotnetcore/DotnetSpider/blob/master/src/DotnetSpider.Extension.Test/nlog.config


### Web Manager

https://github.com/zlzforever/DotnetSpider.Enterprise

1. Dependences a ci platform forexample i used gitlab-ci right now.
2. Dependences Sceduler.NET https://github.com/zlzforever/Scheduler.NET 
3. More documents continue...

![1](https://github.com/dotnetcore/DotnetSpider/raw/master/images/1.png)
![2](https://github.com/dotnetcore/DotnetSpider/raw/master/images/2.png)
![3](https://github.com/dotnetcore/DotnetSpider/raw/master/images/3.png)
![4](https://github.com/dotnetcore/DotnetSpider/raw/master/images/4.png)
![5](https://github.com/dotnetcore/DotnetSpider/raw/master/images/5.png)

### NOTICE

#### when you use redis scheduler, please update your redis config: 
	timeout 0 
	tcp-keepalive 60

### Comments

+ EntitSpider定义的表名和列名全部小写化, 以备不同数据库间转换或者MYSQL win/linux的切换
+ 允许不添加Pipeline执行爬虫

### Buy me a coffe

![](https://github.com/zlzforever/DotnetSpiderPictures/raw/master/pay.png)

### AREAS FOR IMPROVEMENTS

QQ Group: 477731655
Email: zlzforever@163.com