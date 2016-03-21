//using System;
//using System.Collections.Generic;
//using Java2Dotnet.Spider.Common;
//using Java2Dotnet.Spider.Core;

//namespace Java2Dotnet.Spider.Test.Example
//{
//	public class OschinaBlogSpider : BaseTask
//	{
//		[Schema("oschina", "blog", TableSuffix.Today)]
//		public class OschinaBlog : BaseEntity
//		{
//			[PropertyExtractBy(Expression = "//title/text()")]
//			public string Title { get; set; }

//			[PropertyExtractBy(Expression = "div.BlogContent", Type = ExtractType.Css)]
//			public string Content { get; set; }

//			[PropertyExtractBy(Expression = "//div[@class='BlogTags']/a/text()")]
//			public string Tags { get; set; }
//		}

//		protected override SpiderContext CreateSpiderContext()
//		{
//			return new SpiderContext
//			{
//				SpiderName = "Oschina Blog Daliy Tracking " + DateTimeUtils.FirstDayofThisWeek.ToString("yyyy-MM-dd"),
//				Site = new Site
//				{
//					Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
//					UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36",
//					Headers = new Dictionary<string, string>
//					{
//						{ "Upgrade-Insecure-Requests","1" },
//						{ "Cache-Control","max-age=0" }
//					},
//					EncodingName = "UTF-8"
//				},
//				EmptySleepTime = 10000,
//				Scheduler = new QueueScheduler().ToJObject(),
//				StartUrls = new List<string> { "http://my.oschina.net/flashsword/blog?fromerr=XZc4yHVr" },
//				Pipeline = new MysqlFilePipeline().ToJObject()
//			};
//		}
//		public override HashSet<Type> EntiTypes => new HashSet<Type>() { typeof(OschinaBlog) };
//	}

//	[TestClass]
//	public class OschinaBlogTest
//	{
//		[TestMethod]
//		public void TestOschinaBlog()
//		{
//			OschinaBlogSpider spider = new OschinaBlogSpider();

//			spider.Run();
//			//ModelMysqlFileSpider<OschinaBlog> spider = new ModelMysqlFileSpider<OschinaBlog>(Guid.NewGuid().ToString(), new Site
//			//{
//			//	Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
//			//	UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36",
//			//	Headers = new Dictionary<string, string>()
//			//	{
//			//		{ "Upgrade-Insecure-Requests","1" },
//			//		{ "Cache-Control","max-age=0" },
//			//	},
//			//	Encoding = Encoding.UTF8
//			//});
//			//spider.AddStartUrl("http://my.oschina.net/flashsword/blog?fromerr=XZc4yHVr");
//			//spider.Run();


//		}
//	}
//}