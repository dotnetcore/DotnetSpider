using System;
using Java2Dotnet.Spider.Extension;
using Java2Dotnet.Spider.Core;
using System.Collections.Generic;
using Java2Dotnet.Spider.Extension.Configuration;
using Java2Dotnet.Spider.Extension.Model;
using Java2Dotnet.Spider.Extension.Model.Attribute;
using Java2Dotnet.Spider.Extension.ORM;

namespace Java2Dotnet.Spider.Test.Example
{
	public class CnblogsSpider : SpiderBuilder
	{
		protected override SpiderContext GetSpiderContext()
		{
			SpiderContext context = new SpiderContext();
			context.Site = new Site
			{
				MaxSleepTime = 1,
				MinSleepTime = 1
			};
			context.SetTaskGroup("cnblogs homepage");
			context.SetSpiderName("cnblogs homepage " + DateTime.Now.ToString("yyyy-MM-dd HHmmss"));
			context.AddStartUrl("http://news.cnblogs.com/n/page/1/");
			context.AddPipeline(new MysqlPipeline
			{
				ConnectString = "Database='taobao';Data Source= 86research.imwork.net;User ID=root;Password=1qazZAQ!;Port=4306"
			});
			context.AddTargetUrlExtractor(new Extension.Configuration.TargetUrlExtractor
			{
				Region = new Extension.Configuration.Selector
				{
					Expression = "//*[@id='pager']",
					Type = ExtractType.XPath
				}
			});
			context.ThreadNum = 5;
			context.AddEntityType(typeof(Cnblogs));
			return context;
		}

		[Schema("test", "cnblogs", TableSuffix.Today)]
		[TypeExtractBy(Expression = ".//div[@class='news_block']", Multi = true)]
		public class Cnblogs : ISpiderEntity
		{
			[StoredAs("title", DataType.Text)]
			[PropertyExtractBy(Expression = ".//*[@class='news_entry']/a")]
			public string title { get; set; }

			[StoredAs("summary", DataType.Text)]
			[PropertyExtractBy(Expression = ".//div[@class='entry_summary']")]
			public string summary { get; set; }

			[StoredAs("comment", DataType.Text)]
			[PropertyExtractBy(Expression = ".//span[@class='comment']/a")]
			public string comment { get; set; }

			[StoredAs("view", DataType.Text)]
			[PropertyExtractBy(Expression = ".//span[@class='view']")]
			public string view { get; set; }
		}
	}
}