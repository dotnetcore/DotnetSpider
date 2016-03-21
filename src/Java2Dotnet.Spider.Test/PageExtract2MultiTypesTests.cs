//using System;
//using System.Collections.Generic;
//using System.Text;
//using Java2Dotnet.Spider.Core;
//using Java2Dotnet.Spider.Extension.DbSupport;
//using Java2Dotnet.Spider.Extension.Model;
//using Java2Dotnet.Spider.Extension.Model.Attribute;
//using Java2Dotnet.Spider.Extension.Pipeline;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace Java2Dotnet.Spider.Extension.Test
//{
//	[TestClass]
//	public class PageExtract2MultiTypesTests
//	{
//		[TypeExtractBy(Expression = "//*[@id='nav_menu']/a[1]")]
//		[Schema("cnblogs", "yuanzi")]
//		public class Yuanzi : BaseEntity
//		{
//			[PropertyExtractBy(Expression = ".")]
//			[StoredAs("name", StoredAs.ValueType.String, 20)]
//			public string Name { get; set; }
//		}

//		[TypeExtractBy(Expression = "//*[@id='nav_menu']/a[2]")]
//		[Schema("cnblogs", "jinghua")]
//		public class Jinghua : BaseEntity
//		{
//			[StoredAs("id", StoredAs.ValueType.Long, true)]
//			[KeyProperty(Identity = true)]
//			public override long Id { get; set; }

//			[PropertyExtractBy(Expression = ".")]
//			[StoredAs("name", StoredAs.ValueType.Varchar, false, 20)]
//			public string Name { get; set; }
//		}

//		[TestMethod]
//		public void PageExtract2MultiTypes()
//		{
//			ModelCollectorSpider<Yuanzi, Jinghua> spider = new ModelCollectorSpider<Yuanzi, Jinghua>(Guid.NewGuid().ToString(), new Site { SleepTime = 1000, Encoding = Encoding.UTF8 });
//			spider.SetEmptySleepTime(15000);
//			spider.SetThreadNum(1);
//			spider.SetCachedSize(1);
//			spider.AddStartUrls(new List<string> { "http://www.cnblogs.com/" });
//			spider.Run();
//			var results1 = ((CollectorModelPipeline<Yuanzi>)(((ModelPipeline<Yuanzi>)spider.Pipelines[0]).PageModelPipeline)).GetCollected();
//			var results2 = ((CollectorModelPipeline<Jinghua>)(((ModelPipeline<Jinghua>)spider.Pipelines[1]).PageModelPipeline)).GetCollected();
//			Assert.AreEqual("园子", results1[0].Name);
//			Assert.AreEqual("新闻", results2[0].Name);
//		}


//		[TestMethod]
//		public void PageExtract2MultiTypes3()
//		{
//			ModelDatabaseSpider<Yuanzi, Jinghua> spider = new ModelDatabaseSpider<Yuanzi, Jinghua>(Guid.NewGuid().ToString(), new Site { SleepTime = 1000, Encoding = Encoding.UTF8 });
//			spider.SetEmptySleepTime(15000);
//			spider.SetThreadNum(1);
//			spider.SetCachedSize(1);
//			spider.AddStartUrls(new List<string> { "http://www.cnblogs.com/" });
//			spider.Run();
//			DataRepository<Jinghua> dataRepository = new DataRepository<Jinghua>();
//			Assert.AreEqual("新闻", dataRepository.GetWhere("id>0").ToList()[0].Name);
//			dataRepository.Execute("drop database cnblogs");
//		}
//	}
//}
