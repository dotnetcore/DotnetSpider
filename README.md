# DotnetSpider
[![Travis branch](https://travis-ci.org/dotnetcore/DotnetSpider.svg?branch=master)](https://travis-ci.org/dotnetcore/DotnetSpider)
[![NuGet](https://img.shields.io/nuget/v/DotnetSpider.Extension.svg)](https://www.nuget.org/packages/DotnetSpider.Extension)
[![Member project of .NET Core Community](https://img.shields.io/badge/member%20project%20of-NCC-9e20c9.svg)](https://github.com/dotnetcore)
[![GitHub license](https://img.shields.io/aur/license/yaourt.svg)](https://raw.githubusercontent.com/dotnetcore/DotnetSpider/master/LICENSE)

DotnetSpider, a .NET Standard web crawling library similar to WebMagic and Scrapy. It is a lightweight ,efficient and fast high-level web crawling & scraping framework for .NET

### DESIGN

![DESIGN](https://github.com/dotnetcore/DotnetSpider/raw/master/images/DESIGN.jpg)

### DEVELOP ENVIROMENT
- Visual Studio 2017(15.3 or later)
- [.NET Core 2.0 or later](https://www.microsoft.com/net/download/windows)
- Storage data to mysql. [Download MySql](https://dev.mysql.com/downloads/mysql/) 
	
		grant all on *.* to 'root'@'localhost' IDENTIFIED BY '' with grant option;
	
		flush privileges;

### OPTIONAL ENVIROMENT

- Distributed crawler. [Download Redis for windows](https://github.com/MSOpenTech/redis/releases)
- SqlServer.
- PostgreSQL.
- MongoDb

### MORE DOCUMENTS

https://github.com/dotnetcore/DotnetSpider/wiki

### SAMPLES

	Please see the Projet DotnetSpider.Sample in the solution.

### BASE USAGE

[Base usage Codes](https://github.com/zlzforever/DotnetSpider/blob/master/src/DotnetSpider.Sample/BaseUsage.cs)

### ADDITIONAL USAGE: Configurable Entity Spider

[View compelte Codes](https://github.com/zlzforever/DotnetSpider/blob/master/src/DotnetSpider.Sample/JdSkuSampleSpider.cs)

	public class EntityModelSpider
	{
		public static void Run()
		{
			Spider spider = new Spider();
			spider.Run();
		}

		private class Spider : EntitySpider
		{
			protected override void OnInit(params string[] arguments)
			{
				var word = "可乐|雪碧";
				AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });
				AddEntityType<BaiduSearchEntry>();
				AddPipeline(new ConsoleEntityPipeline());
			}

			[TableInfo("baidu", "baidu_search_entity_model")]
			[EntitySelector(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
			class BaiduSearchEntry : BaseEntity
			{
				[Field(Expression = "Keyword", Type = SelectorType.Enviroment)]
				public string Keyword { get; set; }

				[Field(Expression = ".//h3[@class='c-title']/a")]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				public string Title { get; set; }

				[Field(Expression = ".//h3[@class='c-title']/a/@href")]
				public string Url { get; set; }

				[Field(Expression = ".//div/p[@class='c-author']/text()")]
				[ReplaceFormatter(NewValue = "-", OldValue = "&nbsp;")]
				public string Website { get; set; }

				[Field(Expression = ".//div/span/a[@class='c-cache']/@href")]
				public string Snapshot { get; set; }

				[Field(Expression = ".//div[@class='c-summary c-row ']", Option = FieldOptions.InnerText)]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
				public string Details { get; set; }

				[Field(Expression = ".", Option = FieldOptions.InnerText)]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
				public string PlainText { get; set; }
			}
		}
	}

	public static void Main()
	{
		EntityModelSpider.Run();
	}

#### Run via Startup

	Command: -s:[spider type name | TaskName attribute] -i:[identity] -a:[arg1,arg2...] --tid:[taskId] -n:[name] -c:[configuration file path or name]

1. -s: Type name of spider or TaskNameAttribute for example: DotnetSpider.Sample.BaiduSearchSpiderl
2. -i: Set identity.
3. -a: Pass arguments to spider's Run method.
4. --tid: Set task id.
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


### DotnetSpider.Hub

https://github.com/zlzforever/DotnetSpider.Hub

1. Dependences a ci platform forexample i used teamcity right now.
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

### Buy me a coffe

![](https://github.com/zlzforever/DotnetSpiderPictures/raw/master/pay.png)

### AREAS FOR IMPROVEMENTS

QQ Group: 477731655
Email: zlzforever@163.com
